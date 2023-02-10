using ServiceControlExporter.ServiceControl;
using ServiceControlExporter.Metrics;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace ServiceControlExporter.Tests.Integration;

public class MetricsEndpointTest : IntegrationTest
{
    [SetUp]
    public void Setup()
    {
        // Setup application per test to have a clear baseline
        SetupApplication();
    }

    [Test]
    public async Task Returns_Count_Of_Messages_Per_Endpoint()
    {
        var api = ServiceControlExporterWebApplicationFactory.MockServiceControlApi;
        api.Setup(m => m.GetErrorMessageSummaryPerEndpoint()).ReturnsAsync(new [] {new EndpointGroup { Title = "EndpointName", Count = 1 }});

        var metrics = ServiceControlExporterWebApplicationFactory.Server.Services.GetRequiredService<ObservableMetrics>();
        await metrics.Update(CancellationToken.None).ConfigureAwait(false);

        var response = await ServiceControlExporterClient.GetAsync("/metrics").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        content.Should().Contain("servicecontrolexporter_failed_messages_per_endpoint_messages{endpoint=\"EndpointName\"} 1");
    }

    [Test]
    public async Task Returns_Count_Of_Messages_Per_MessageType()
    {
        var api = ServiceControlExporterWebApplicationFactory.MockServiceControlApi;
        api.Setup(m => m.GetErrorMessageSummaryPerType()).ReturnsAsync(new [] { new EndpointGroup { Title = "MessageType", Count = 1 }});

        var metrics = ServiceControlExporterWebApplicationFactory.Server.Services.GetRequiredService<ObservableMetrics>();
        await metrics.Update(CancellationToken.None).ConfigureAwait(false);

        var response = await ServiceControlExporterClient.GetAsync("/metrics").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        content.Should().Contain("servicecontrolexporter_failed_messages_per_type_messages{type=\"MessageType\"} 1");
    }

    [Test]
    public async Task Returns_0_As_Count_For_Endpoint_That_Previously_Had_Messages_And_Is_No_Longer_Reported_By_The_Api()
    {
        var api = ServiceControlExporterWebApplicationFactory.MockServiceControlApi;
        api.Setup(m => m.GetErrorMessageSummaryPerEndpoint()).ReturnsAsync(new [] {new EndpointGroup { Title = "EndpointName", Count = 1 }});
        api.Setup(m => m.GetErrorMessageSummaryPerType()).ReturnsAsync(new [] { new EndpointGroup { Title = "MessageType", Count = 1 }});

        var metrics = ServiceControlExporterWebApplicationFactory.Server.Services.GetRequiredService<ObservableMetrics>();
        await metrics.Update(CancellationToken.None).ConfigureAwait(false);

        var response = await ServiceControlExporterClient.GetAsync("/metrics").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        // Simulate retried or deleted messages
        api.Setup(m => m.GetErrorMessageSummaryPerEndpoint()).ReturnsAsync(Array.Empty<EndpointGroup>());
        api.Setup(m => m.GetErrorMessageSummaryPerType()).ReturnsAsync(Array.Empty<EndpointGroup>());
        await metrics.Update(CancellationToken.None).ConfigureAwait(false);

        // get the new metrics
        response = await ServiceControlExporterClient.GetAsync("/metrics").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        content.Should().Contain("servicecontrolexporter_failed_messages_per_type_messages{type=\"MessageType\"} 0");
        content.Should().Contain("servicecontrolexporter_failed_messages_per_endpoint_messages{endpoint=\"EndpointName\"} 0");
    }
}