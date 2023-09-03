using Fuse8_ByteMinds.SummerSchool.InternalApi;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var app = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder
            .UseStartup<Startup>()
            .UseKestrel((builderContext, options) =>
            {
                var grpcPort = builderContext.Configuration.GetValue<int>("GrpcPort");

                options.ConfigureEndpointDefaults(p =>
                {
                    p.Protocols = p.IPEndPoint!.Port == grpcPort
                        ? HttpProtocols.Http2
                        : HttpProtocols.Http1;
                });
            })
            .UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration)
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .WriteTo.Console();
            });
    }).Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<CurrencyRateDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

await app.RunAsync();
// TODO: сделать серилог через конфиг