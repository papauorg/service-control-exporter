using System.Diagnostics.Metrics;

using ServiceControlExporter.ServiceControl;

namespace ServiceControlExporter.Metrics;

public sealed class ObservableMetrics
{
    public IReadOnlyList<EndpointGroup> MessagesPerEndpoint { get; private set; } = Array.Empty<EndpointGroup>();
    public IReadOnlyList<EndpointGroup> MessagesPerMessageType { get; private set; } = Array.Empty<EndpointGroup>();

    public IServiceControlApi ServiceControlApi { get; }
    public ILogger<ObservableMetrics> Logger { get; }

    public ObservableMetrics(IServiceControlApi serviceControlApi, ILogger<ObservableMetrics> logger)
    {
        ServiceControlApi = serviceControlApi ?? throw new ArgumentNullException(nameof(serviceControlApi));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Update(CancellationToken cancellationToken)
    {
       MessagesPerEndpoint = await GetErrorMessagesCounterPerEndpoint().ConfigureAwait(false);
       MessagesPerMessageType = await GetErrorMessagesCounterPerType().ConfigureAwait(false);
    }

    private async Task<EndpointGroup[]> GetErrorMessagesCounterPerType()
    {
        try
        {
            var result = (await ServiceControlApi.GetErrorMessageSummaryPerType().ConfigureAwait(false)).ToArray();
            Logger.LogInformation("Retrieved {measurements} measurements for messages per type.", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to retrieve error message counter by type.");
        }

        return Array.Empty<EndpointGroup>();
    }

    private async Task<EndpointGroup[]> GetErrorMessagesCounterPerEndpoint()
    {
        try
        {
            var result =  (await ServiceControlApi.GetErrorMessageSummaryPerEndpoint().ConfigureAwait(false)).ToArray();
            Logger.LogInformation("Retrieved {measurements} measurements for messages per endpoint.", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to retrieve error message counter by Endpoint.");
        }

        return Array.Empty<EndpointGroup>();
    }

}