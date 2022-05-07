using IoTGatewayService;
using IoTGatewayService.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(builder => builder.AddSeq());
builder.Services.AddGrpc();
builder.Services.AddSingleton<DeviceClient>();
builder.Services.AddOpenTelemetryTracing(otbilder =>
{
    otbilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("IoTGatewayService"))
        .AddAspNetCoreInstrumentation()
        .AddSource(nameof(GatewayService)) // when we manually create activities, we need to setup the sources here
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "localhost";
            options.AgentPort = 6831;
        })
        .AddConsoleExporter();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GatewayService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();