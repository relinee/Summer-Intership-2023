using Fuse8_ByteMinds.SummerSchool.InternalApi;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Events;

await Host.CreateDefaultBuilder(args)
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
    }).Build()
    .RunAsync();

// TODO: сделать серилог через конфиг