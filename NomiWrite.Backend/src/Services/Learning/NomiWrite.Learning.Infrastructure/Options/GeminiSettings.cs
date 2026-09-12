namespace NomiWrite.Learning.Infrastructure.Options;

public class GeminiSettings
{
    public const string SectionName = "GeminiSettings";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash";
    public string Endpoint { get; set; } =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
}