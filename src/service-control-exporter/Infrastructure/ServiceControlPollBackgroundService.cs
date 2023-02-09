using ServiceControlExporter.Metrics;

namespace ServiceControlExporter.Infrastructure;

public class ServiceControlPollBackgroundService : BackgroundService
{
    public IServiceProvider ServiceProvider { get; }
    public ServiceControlOptions Options { get; }
    public ILogger<ServiceControlPollBackgroundService> Logger { get; }

    public ServiceControlPollBackgroundService(IServiceProvider serviceProvider, ServiceControlOptions options, ILogger<ServiceControlPollBackgroundService> logger)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Options.RequestInterval <= TimeSpan.Zero)
        {
            Logger.LogWarning("Requesting metrics from ServiceControl is disabled. Set a request interval > 0. Current: {requestInterval}", Options.RequestInterval);
            return;
        }

        Logger.LogInformation("Setting up service control request with interval: {requestInterval}", Options.RequestInterval);

        var timer = new PeriodicTimer(Options.RequestInterval);

        do
        {
            Logger.LogDebug("Attempting to update metrics from ServiceControl.");
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var metrics = scope.ServiceProvider.GetRequiredService<ObservableMetrics>();
                await metrics.Update(stoppingToken).ConfigureAwait(false);
                Logger.LogDebug("Metric updated.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Can't update metrics from ServiceControl.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }
}

