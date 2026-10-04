namespace NomiWrite.AICoordinator.Application.DTOs;

public class AiGradingConfigDto
{
    public Guid Id { get; set; }
    public string ProviderName { get; set; } = "Gemini";
    public string ModelName { get; set; } = string.Empty;
    public string? FallbackModelName { get; set; }
    public bool HasStoredApiKey { get; set; }
    public string? ApiKeyHint { get; set; }
    public decimal? Temperature { get; set; }
    public string? SystemPromptTemplate { get; set; }
    public int? MaxOutputTokens { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateAiGradingConfigRequestDto
{
    public string ProviderName { get; set; } = "Gemini";
    public string ModelName { get; set; } = string.Empty;
    public string? FallbackModelName { get; set; }
    public string? ApiKey { get; set; }
    public bool ClearApiKey { get; set; }
    public decimal? Temperature { get; set; }
    public string? SystemPromptTemplate { get; set; }
    public int? MaxOutputTokens { get; set; }
}
