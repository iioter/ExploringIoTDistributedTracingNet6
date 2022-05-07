using EasyNetQ;
using LogService;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(builder => builder.AddSeq());
        services.AddSingleton<IBus>(_ => RabbitHutch.CreateBus("host=localhost"));
        services.AddLogging(builder => builder.AddSeq());
        services.AddOpenTelemetryTracing(builder =>
        {
            builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("LogService"))
                .AddSource(nameof(LogHandler)) // when we manually create activities, we need to setup the sources here
                .AddJaegerExporter(options =>
                {
                    options.AgentHost = "localhost";
                    options.AgentPort = 6831;
                })
                .AddConsoleExporter();
        });
        services.AddHostedService<LogHandler>();
    })
    .Build();

await host.RunAsync();

public record HelloMessage(string Message);
