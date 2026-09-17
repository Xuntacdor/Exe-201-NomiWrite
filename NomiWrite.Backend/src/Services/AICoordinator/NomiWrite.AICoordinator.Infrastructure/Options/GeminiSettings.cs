namespace NomiWrite.AICoordinator.Infrastructure.Options;

public class GeminiSettings
{
    public const string SectionName = "GeminiSettings";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3.6-flash";
    public string Endpoint { get; set; } = string.Empty;
}
