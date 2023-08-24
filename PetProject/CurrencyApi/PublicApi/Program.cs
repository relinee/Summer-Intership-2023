using Fuse8_ByteMinds.SummerSchool.PublicApi;
using Microsoft.AspNetCore;
using Serilog;
using Serilog.Events;

public class Program
{
	static void Main(string[] args)
	{
		CreateWebHostBuilder(args)
			.Build()
			.Run();
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