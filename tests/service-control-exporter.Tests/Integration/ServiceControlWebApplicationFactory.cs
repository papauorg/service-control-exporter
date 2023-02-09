using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using ServiceControlExporter.ServiceControl;
using ServiceControlExporter.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace ServiceControlExporter.Tests.Integration;

public class ServiceControlExporterWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IServiceControlApi> MockServiceControlApi { get; set; } = new Mock<IServiceControlApi>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // replace ServiceControl api with a mock
            var serviceControlApi = services.Single(
                d => d.ServiceType == typeof(IServiceControlApi));
            services.Remove(serviceControlApi!);
            services.AddSingleton<IServiceControlApi>(MockServiceControlApi.Object);

            // remove hosted service to avoid timer interfering with tests
            var hostedService = services.Single(d => d.ImplementationType == typeof(ServiceControlPollBackgroundService));
        });

        builder.UseEnvironment("Testing");
    }
}