using Fuse8_ByteMinds.SummerSchool.PublicApi;
using Microsoft.AspNetCore;
using Serilog;
using Serilog.Events;

var webHost = WebHost
	.CreateDefaultBuilder(args)
	.UseStartup<Startup>()
	.UseSerilog((ctx, cfg) =>
	{
		cfg.ReadFrom.Configuration(ctx.Configuration)
			.MinimumLevel.Debug()
			.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
			.WriteTo.Console();
	})
	.Build();


await webHost.RunAsync();