namespace NomiWrite.Learning.Infrastructure.Options;

public class GroqSettings
{
    public const string SectionName = "GroqSettings";

    public string ApiKey { get; set; } = string.Empty;
    public string Models { get; set; } = "openai/gpt-oss-120b";
    public string Endpoint { get; set; } = "https://api.groq.com/openai/v1/chat/completions";
}
