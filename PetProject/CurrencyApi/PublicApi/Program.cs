using Fuse8_ByteMinds.SummerSchool.PublicApi;
using Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

public class Program
{
	static void Main(string[] args)
	{
		var app = CreateWebHostBuilder(args)
			.Build();
		using (var scope = app.Services.CreateScope())
		{
			var services = scope.ServiceProvider;

			var context = services.GetRequiredService<CurrencyFavouritesAndSettingsDbContext>();
			if (context.Database.GetPendingMigrations().Any())
			{
				context.Database.Migrate();
			}
		}
		app.Run();
	}
  
	public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
		WebHost.CreateDefaultBuilder()
			.UseStartup<Startup>()
			.UseSerilog((context, configuration) =>
			{
				configuration.ReadFrom.Configuration(context.Configuration)
					.MinimumLevel.Debug()
					.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
					.WriteTo.Console();
			});
}