using OpenTelemetry;
using OpenTelemetry.Metrics;

using Serilog;

using ServiceControlExporter;
using ServiceControlExporter.Infrastructure;
using ServiceControlExporter.Metrics;
using ServiceControlExporter.ServiceControl;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Host.UseWindowsService();

var options = builder.Configuration.GetSection("ServiceControlOptions").Get<ServiceControlOptions>()!;
builder.Services.AddSingleton<ServiceControlOptions>(options);
builder.Services.AddSingleton<ObservableMetrics>();
builder.Services.AddSingleton<ServiceControlMeter>();
builder.Services.AddTransient<IServiceControlApi, ServiceControlApi>();
builder.Services.AddHostedService<ServiceControlPollBackgroundService>();

builder.Services.AddHttpClient(ServiceControlApi.HTTP_CLIENT_NAME, c => {
    c.BaseAddress = new Uri(options.BaseUrl);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Add services to the container.
builder.Services.AddOpenTelemetry()
    .WithMetrics(builder => builder
        .AddPrometheusExporter()
        .AddMeter(ServiceControlMeter.NAME))
    .StartWithHost();

var app = builder.Build();

// make sure meter gets created at least once
var meter = app.Services.GetRequiredService<ServiceControlMeter>();

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.Run();


public partial class Program {}