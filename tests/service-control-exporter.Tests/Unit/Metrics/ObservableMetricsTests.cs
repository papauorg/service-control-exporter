using ServiceControlExporter.Metrics;
using ServiceControlExporter.ServiceControl;
using Microsoft.Extensions.Logging;

namespace ServiceControlExporter.Tests.Unit.Metrics;

public class ObservableMetricsTests
{
    protected ObservableMetrics _metrics;
    protected Mock<IServiceControlApi> _api;

    [SetUp]
    public void Setup()
    {
        _api = new Mock<IServiceControlApi>();
        _metrics = new ObservableMetrics(_api.Object, Mock.Of<ILogger<ObservableMetrics>>());
    }

    public class UpdateMethod : ObservableMetricsTests
    {
        [Test]
        public async Task Fills_The_Dictionaries_With_Current_Values_By_The_Api()
        {
            _api.Setup(m => m.GetErrorMessageSummaryPerEndpoint()).ReturnsAsync(new [] {new EndpointGroup { Title = "EndpointName", Count = 1 }});
            _api.Setup(m => m.GetErrorMessageSummaryPerType()).ReturnsAsync(new [] { new EndpointGroup { Title = "MessageType", Count = 1 }});
            
            await _metrics.Update(CancellationToken.None).ConfigureAwait(false);

            _metrics.MessagesPerEndpoint.Should().Contain("EndpointName", 1);
            _metrics.MessagesPerMessageType.Should().Contain("MessageType", 1);
        }

        [Test]
        public async Task Overwrites_Existing_Values()
        {
            _metrics.MessagesPerEndpoint["EndpointName"] = 5;
            _metrics.MessagesPerMessageType["MessageType"] = 5;
            _api.Setup(m => m.GetErrorMessageSummaryPerEndpoint()).ReturnsAsync(new [] {new EndpointGroup { Title = "EndpointName", Count = 1 }});
            _api.Setup(m => m.GetErrorMessageSummaryPerType()).ReturnsAsync(new [] { new EndpointGroup { Title = "MessageType", Count = 1 }});

            await _metrics.Update(CancellationToken.None).ConfigureAwait(false);

            _metrics.MessagesPerEndpoint.Should().Contain("EndpointName", 1);
            _metrics.MessagesPerMessageType.Should().Contain("MessageType", 1);
        }

        [Test]
        public async Task Resets_Count_To_0_If_Values_Per_Endpoint_Or_Message_Are_No_Longer_Returned_By_The_Api()
        {
            _metrics.MessagesPerEndpoint["EndpointName"] = 5;
            _metrics.MessagesPerMessageType["MessageType"] = 5;
            _api.Setup(m => m.GetErrorMessageSummaryPerEndpoint()).ReturnsAsync(Array.Empty<EndpointGroup>());
            _api.Setup(m => m.GetErrorMessageSummaryPerType()).ReturnsAsync(Array.Empty<EndpointGroup>());

            await _metrics.Update(CancellationToken.None).ConfigureAwait(false);

            _metrics.MessagesPerEndpoint.Should().Contain("EndpointName", 0);
            _metrics.MessagesPerMessageType.Should().Contain("MessageType", 0);
        }
    }
}