namespace ServiceControlExporter;

public record ServiceControlOptions
{
    public string BaseUrl { get; init; } = "http://localhost:33333";
    public TimeSpan RequestInterval { get; init; } = TimeSpan.Zero;
}