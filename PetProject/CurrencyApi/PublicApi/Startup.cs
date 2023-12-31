﻿using System.Reflection;
using System.Text.Json.Serialization;
using Audit.Core;
using Audit.Http;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Filter;
using Fuse8_ByteMinds.SummerSchool.PublicApi.GrpcContracts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Middlewares;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.OpenApi.Models;


namespace Fuse8_ByteMinds.SummerSchool.PublicApi;

public class  Startup
{
	private readonly IConfiguration _configuration;

	public Startup(IConfiguration configuration)
	{
		_configuration = configuration;
	}
	
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddControllers()

			// Добавляем глобальные настройки для преобразования Json
			.AddJsonOptions(
				options =>
				{
					// Добавляем конвертер для енама
					// По умолчанию енам преобразуется в цифровое значение
					// Этим конвертером задаем перевод в строковое занчение
					options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
				});
		;
		
		// Логгирование входящих запросов (альтернатива мидлваре)
		// services.AddHttpLogging(logger =>
		// {
		// 	logger.LoggingFields = HttpLoggingFields.RequestPath;
		// });

		// Регистрация сервисов
		services.AddScoped<ICurrencyService, CurrencyService>();
		services.AddScoped<ICurrencyFavouriteService, CurrencyFavouriteService>();
		services.AddScoped<ICurrencyServiceByFavouriteName, CurrencyService>();

		// Регистрация gRPC клиента
		services.AddGrpcClient<GrpcCurrency.GrpcCurrencyClient>(o =>
			{
				o.Address = new Uri(_configuration["CurrencyGrpcServerAddress"]);
			})
			.AddAuditHandler(audit => audit
				.IncludeRequestBody()
				.IncludeRequestHeaders()
				.IncludeResponseBody()
				.IncludeResponseHeaders()
				.IncludeContentHeaders());

		Configuration.Setup()
			.UseSerilog(
				config => config.Message(
					auditEvent =>
					{
						if (auditEvent is AuditEventHttpClient httpClientEvent)
						{
							var contentBody = httpClientEvent.Action?.Response?.Content?.Body;
							if (contentBody is string { Length: > 1000 } stringBody)
							{
								httpClientEvent.Action.Response.Content.Body = stringBody[..1000] + "<...>";
							}
						}
						return auditEvent.ToJson();
					}));
		
		// Регистрация DbContext
		services.AddDbContext<CurrencyFavouritesAndSettingsDbContext>(
			optionsBuilder =>
			{
				optionsBuilder
					.UseNpgsql(
						connectionString: _configuration.GetConnectionString("currency_api"),
						npgsqlOptionsAction: sqlOptionsBuilder =>
						{
							// Переподключение при ошибке
							sqlOptionsBuilder.EnableRetryOnFailure();
							// Добавление истории миграций
							sqlOptionsBuilder.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "user");
						}
					).UseAllCheckConstraints() // Проверка ограничений таблицы
					.UseSnakeCaseNamingConvention(); // Нейминг в виде снейк кейса
			}
		);

		// Добавление фильтра ошибок
		services.AddControllers(options =>
		{
			options.Filters.Add(typeof(ApiExceptionFilter));
		});

		// Регистрация конфигурации
		var sectionApiSettings = _configuration.GetRequiredSection("ApiSettings");
		services.Configure<ApiSettings>(sectionApiSettings);
		
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Version = "v1",
				Title = "Currency Public API",
				Description = "Внешний сервис по работе с курсами валют"
			});
			var basePath = AppContext.BaseDirectory;
			var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			var xmlPath = Path.Combine(basePath, xmlFile);
			options.IncludeXmlComments(xmlPath);
		});
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}
		
		app.UseMiddleware<IncomingRequestsLoggingMiddleware>();
		app.UseRouting()
			.UseEndpoints(endpoints => endpoints.MapControllers());
	}
}