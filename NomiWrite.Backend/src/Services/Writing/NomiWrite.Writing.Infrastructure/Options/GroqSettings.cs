namespace NomiWrite.Writing.Infrastructure.Options;

public class GroqSettings
{
    public const string SectionName = "GroqSettings";

    public string ApiKey { get; set; } = string.Empty;

    // Ordered, comma-separated model chain. Benchmarked for autocomplete:
    // qwen/qwen3.8-27b (best quality, ~300ms p50) then allam-2-7b (fast fallback).
    public string Models { get; set; } = "qwen/qwen3.8-27b,allam-2-7b";

    public string Endpoint { get; set; } = "https://api.groq.com/openai/v1/chat/completions";
}
