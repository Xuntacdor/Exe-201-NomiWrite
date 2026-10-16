namespace NomiWrite.Learning.Infrastructure.Options;

public class OpenRouterSettings
{
    public const string SectionName = "OpenRouterSettings";

    public string ApiKey { get; set; } = string.Empty;
    public string Models { get; set; } = "nvidia/nemotron-3.5-lightning:free,nvidia/nemotron-3-super-120b-a12b:free,google/gemma-4-31b-it:free,nvidia/nemotron-3-nano-omni-30b-a3b-reasoning:free,liquid/lfm-2.5-2.6b:free";
    public string Endpoint { get; set; } = "https://openrouter.ai/api/v1/chat/completions";
}
