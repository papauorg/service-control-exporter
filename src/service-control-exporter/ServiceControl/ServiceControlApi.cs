namespace ServiceControlExporter.ServiceControl;

internal class ServiceControlApi : IServiceControlApi
{
    public const string HTTP_CLIENT_NAME = "ServiceControlApiClient";

    public IHttpClientFactory HttpClientFactory { get; }

    public ServiceControlApi(IHttpClientFactory httpClientFactory)
    {
        HttpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<IEnumerable<EndpointGroup>> GetErrorMessageSummaryPerEndpoint()
    {
        var client = HttpClientFactory.CreateClient(HTTP_CLIENT_NAME);
        var result = await client.GetFromJsonAsync<EndpointGroup[]>("api/recoverability/groups/Endpoint%20Name").ConfigureAwait(false);
        return result ?? Array.Empty<EndpointGroup>();
    }

    public async Task<IEnumerable<EndpointGroup>> GetErrorMessageSummaryPerType()
    {
        var client = HttpClientFactory.CreateClient(HTTP_CLIENT_NAME);
        var result = await client.GetFromJsonAsync<EndpointGroup[]>("api/recoverability/groups/Message%20Type").ConfigureAwait(false);
        return result ?? Array.Empty<EndpointGroup>();
    }
}
