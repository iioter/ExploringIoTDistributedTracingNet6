using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(builder => builder.AddSeq());
// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpClient();

builder.Services.AddOpenTelemetryTracing(otbuilder =>
{
    otbuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebApp"))
        .AddAspNetCoreInstrumentation(
            // if we wanted to ignore some specific requests, we could use the filter
            options => options.Filter = httpContext => !httpContext.Request.Path.Value?.Contains("/_framework/aspnetcore-browser-refresh.js") ?? true)
        .AddHttpClientInstrumentation(
            // we can hook into existing activities and customize them
            options => options.Enrich = (activity, eventName, rawObject) =>
            {
                if (eventName == "OnStartActivity" && rawObject is HttpRequestMessage request && request.Method == HttpMethod.Get)
                {
                    activity.SetTag("RandomDemoTag", "Adding some random demo tag, just to see things working");
                }
            }
        )
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "localhost";
            options.AgentPort = 6831;
        })
        .AddConsoleExporter();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
