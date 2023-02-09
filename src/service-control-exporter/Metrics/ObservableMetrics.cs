using System.Diagnostics.Metrics;
using System.Collections.Concurrent;
using ServiceControlExporter.ServiceControl;

namespace ServiceControlExporter.Metrics;

public sealed class ObservableMetrics
{
    public ConcurrentDictionary<string, int> MessagesPerEndpoint { get; } = new ConcurrentDictionary<string, int>();
    public ConcurrentDictionary<string, int> MessagesPerMessageType { get; } = new ConcurrentDictionary<string, int>();

    public IServiceControlApi ServiceControlApi { get; }
    public ILogger<ObservableMetrics> Logger { get; }

    public ObservableMetrics(IServiceControlApi serviceControlApi, ILogger<ObservableMetrics> logger)
    {
        ServiceControlApi = serviceControlApi ?? throw new ArgumentNullException(nameof(serviceControlApi));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Update(CancellationToken cancellationToken)
    {
       var perEndpointList = await GetErrorMessagesCounterPerEndpoint().ConfigureAwait(false);
       UpdateDictionary(MessagesPerEndpoint, perEndpointList);

       var perMessageTypeList = await GetErrorMessagesCounterPerType().ConfigureAwait(false);
       UpdateDictionary(MessagesPerMessageType, perMessageTypeList);
    }

    private void UpdateDictionary(ConcurrentDictionary<string, int> dict, IEnumerable<EndpointGroup> updatedValues)
    {
        var existingKeys = dict.Keys;
        var updatedKeys = updatedValues.Select(u => u.Title).ToArray();
        var allKeys = existingKeys.Union(updatedKeys);

        foreach(var key in allKeys)
        {
            // reset count of messages for a given title to 0 in case it is no longer
            // returned by the ServiceControl api. Otherwise the initial count of messages
            // would remain the same after messages are deleted or resolved in ServicePulse
            EndpointGroup groupValue = new EndpointGroup { Title = key, Count = 0 };
            groupValue = updatedValues.SingleOrDefault(u => u.Title.Equals(key, StringComparison.Ordinal)) ?? groupValue;
            dict.AddOrUpdate(key, groupValue.Count, (key, oldValue) => groupValue.Count);
        }
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