using System.Reflection;
using System.Text.Json.Serialization;
using Audit.Core;
using Audit.Http;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Filter;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Middlewares;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi;

public class Startup
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

		// Регистрация сервисов
		services.AddTransient<ICurrencyRestService, CurrencyRestService>();

		services.AddHttpClient<ICurrencyAPI, CurrencyService>()
			.AddAuditHandler(audit => audit
				.IncludeRequestBody()
				.IncludeRequestHeaders()
				.IncludeResponseBody()
				.IncludeResponseHeaders()
				.IncludeContentHeaders());
		
		services.AddTransient<ICachedCurrencyAPI, CachedCurrencyService>();

		// Регистрация gRPC сервиса
		services.AddGrpc();

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

		// Добавление фильтра ошибок
		services.AddControllers(options =>
		{
			options.Filters.Add(typeof(ApiExceptionFilter));
		});

		// Регистрация конфигурации
		var sectionApiSettings = _configuration.GetRequiredSection("ApiSettings");
		services.Configure<ApiSettings>(sectionApiSettings);
		
		var sectionCacheSettings = _configuration.GetRequiredSection("CurrencyCacheSettings");
		services.Configure<CurrencyCacheSettings>(sectionCacheSettings);
		
		// Регистрация DbContext
		services.AddDbContext<CurrencyRateDbContext>(
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
							sqlOptionsBuilder.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "cur");
						}
					).UseSnakeCaseNamingConvention(); // Нейминг в виде снейк кейса
			}
		);
		
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Version = "v1",
				Title = "Currency Internal API",
				Description = "Внутренний сервис по работе с курсами валют"
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

		// Настройка роутинга и эндпоинтов для gRPC сервиса
		app.UseWhen(
			predicate: context => context.Connection.LocalPort == _configuration.GetValue<int>("GrpcPort"),
			configuration: grpcBuilder =>
			{
				grpcBuilder.UseRouting();
				grpcBuilder.UseEndpoints(endpoints => endpoints.MapGrpcService<GrpcCurrencyService>());
			});

		app.UseMiddleware<IncomingRequestsLoggingMiddleware>();
		app.UseRouting()
			.UseEndpoints(endpoints => endpoints.MapControllers());
	}
}