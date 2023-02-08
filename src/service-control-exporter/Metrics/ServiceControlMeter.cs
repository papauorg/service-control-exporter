using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

using ServiceControlExporter.ServiceControl;

namespace ServiceControlExporter.Metrics;

internal class ServiceControlMeter
{
    public const string NAME = "ServiceControlExporter.ServiceControlMeter";

    private Meter Meter { get; }
    private ObservableGauge<int> ErrorMessagesPerEndpoint { get; }
    private ObservableGauge<int> ErrorMessagesPerMessageType { get; }
    public ObservableMetrics ObservableMetrics { get; }

    public ServiceControlMeter(ObservableMetrics observableMetrics)
    {
        ObservableMetrics = observableMetrics ?? throw new ArgumentNullException(nameof(observableMetrics));

        Meter = new(ServiceControlMeter.NAME, "1.0");
        ErrorMessagesPerEndpoint = Meter.CreateObservableGauge("ErrorMessagesPerEndpoint", () => ToMeasurements(ObservableMetrics.MessagesPerEndpoint, "endpoint"), unit: "Messages", description: "Shows the amount of error messages per endpoint that require manual review.");
        ErrorMessagesPerMessageType = Meter.CreateObservableGauge("ErrorMessagesPerType", () => ToMeasurements(ObservableMetrics.MessagesPerMessageType, "type"), unit: "Messages", description: "Shows the amount of error messages per message type that require manual review.");
    }

    private static IEnumerable<Measurement<int>> ToMeasurements(IEnumerable<EndpointGroup> endpointGroups, string tagName)
    {
        return endpointGroups
            .Select(s => new Measurement<int>(s.Count, new KeyValuePair<string, object?>[] {new(tagName, s.Title)}))
            .DefaultIfEmpty(new Measurement<int>(0));
    }
}