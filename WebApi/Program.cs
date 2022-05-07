using IoTGatewayService;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(builder => builder.AddSeq());
// Add services to the container.

builder.Services.AddDbContext<ApiDbContext>(options => options.UseNpgsql("server=localhost;port=5432;user id=iot;password=iotgateway;database=webapi"));
builder.Services.AddGrpcClient<Greeter.GreeterClient>(options =>
{
    options.Address = new Uri("http://localhost:5003");
});
builder.Services.AddOpenTelemetryTracing(otbuilder =>
{
    otbuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebApi"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        // to avoid double activity, one for HttpClient, another for the gRPC client
        // -> https://github.com/open-telemetry/opentelemetry-dotnet/blob/core-1.1.0/src/OpenTelemetry.Instrumentation.GrpcNetClient/README.md#suppressdownstreaminstrumentation
        .AddGrpcClientInstrumentation(options => options.SuppressDownstreamInstrumentation = true)
        // besides instrumenting EF, we also want the queries to be part of the telemetry (hence SetDbStatementForText = true)
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddSource(nameof(MessagePublisher)) // when we manually create activities, we need to setup the sources here
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "localhost";
            options.AgentPort = 6831;
        })
        .AddConsoleExporter();
});

builder.Services.AddSingleton<MessagePublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGet("/api", async (string temperature, ApiDbContext db, Greeter.GreeterClient greeterClient,MessagePublisher messagePublisher) =>
{
    var response = await greeterClient.GatewaySetValueAsync(new SetRequest { Value = uint.Parse(temperature) });
    await messagePublisher.PublishAsync(new LogMessage(response.Message));
    db.Set<ApiLogEntry>().Add(
        new ApiLogEntry(Guid.NewGuid(), ushort.Parse(temperature), response.Success, response.Message, DateTime.UtcNow)
    );
    await db.SaveChangesAsync();
    return new LogMessage($"set value:{temperature} ,success:{response.Success},message:{response.Message}");
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

public record LogMessage(string Message);


public record ApiLogEntry(Guid Id, ushort SetValue, bool IsSuccess, string Message, DateTime TimeStamp);
public class ApiDbContext : DbContext
{

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }


    public DbSet<ApiLogEntry>? ApiLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }
}