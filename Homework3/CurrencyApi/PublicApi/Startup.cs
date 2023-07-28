using System.Reflection;
using System.Text.Json.Serialization;
using Audit.Core;
using Audit.Core.Providers;
using Audit.Http;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Handlers;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Middleware;
using Microsoft.OpenApi.Models;


namespace Fuse8_ByteMinds.SummerSchool.PublicApi;

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
		
		string auditLogPath = Path.Combine(Directory.GetCurrentDirectory(), "AuditLogs");
		Configuration.Setup()
			.UseFileLogProvider(config => config.Directory(auditLogPath));
		
		// services.AddHttpClient<CurrencyController>()
		// 	.AddAuditHandler(audit => audit
		// 		.IncludeRequestBody()
		// 		.IncludeRequestHeaders()
		// 		.IncludeResponseBody()
		// 		.IncludeResponseHeaders()
		// 		.IncludeContentHeaders());
		
		services.AddHttpClient("AuditHttpClient")
			.AddAuditHandler(audit => audit
				.IncludeRequestBody()
				.IncludeRequestHeaders()
				.IncludeResponseBody()
				.IncludeResponseHeaders()
				.IncludeContentHeaders());

		services.AddControllers(options =>
		{
			options.Filters.Add(typeof(ApiExceptionFilter));
		});
		
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Version = "v1",
				Title = "Currency API",
				Description = "Пример ASP .NET Core Web API"
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
		app.UseMiddleware<ApiRateLimitMiddleware>();
		app.UseRouting()
			.UseEndpoints(endpoints => endpoints.MapControllers());
	}
}