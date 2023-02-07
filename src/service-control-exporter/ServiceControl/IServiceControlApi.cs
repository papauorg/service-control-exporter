namespace ServiceControlExporter.ServiceControl;

internal interface IServiceControlApi
{
    Task<IEnumerable<EndpointGroup>> GetErrorMessageSummaryPerEndpoint();
    Task<IEnumerable<EndpointGroup>> GetErrorMessageSummaryPerType();
}