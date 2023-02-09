using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace ServiceControlExporter.Tests.Integration;

public class IntegrationTest
{
    protected ServiceControlExporterWebApplicationFactory ServiceControlExporterWebApplicationFactory { get; private set; }
    protected HttpClient ServiceControlExporterClient { get; private set; }

    protected void SetupApplication()
    {
        ServiceControlExporterWebApplicationFactory = new ServiceControlExporterWebApplicationFactory();
        ServiceControlExporterClient = ServiceControlExporterWebApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }
}
