namespace ServiceControlExporter.ServiceControl;

public record EndpointGroup
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; init; } = string.Empty;
    public int Count { get; init; } = 0;
}