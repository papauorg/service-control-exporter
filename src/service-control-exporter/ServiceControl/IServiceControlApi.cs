namespace ServiceControlExporter.ServiceControl;

public interface IServiceControlApi
{
    Task<IEnumerable<EndpointGroup>> GetErrorMessageSummaryPerEndpoint();
    Task<IEnumerable<EndpointGroup>> GetErrorMessageSummaryPerType();
}