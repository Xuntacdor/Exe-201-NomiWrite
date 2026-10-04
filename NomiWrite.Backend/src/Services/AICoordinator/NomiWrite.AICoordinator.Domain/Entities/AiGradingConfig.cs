using NomiWrite.AICoordinator.Domain.Common;

namespace NomiWrite.AICoordinator.Domain.Entities;

public class AiGradingConfig : BaseEntity
{
    public string ProviderName { get; set; } = "Gemini";
    public string ModelName { get; set; } = string.Empty;
    public string? FallbackModelName { get; set; }
    public string? ApiKeyCiphertext { get; set; }
    public decimal? Temperature { get; set; }
    public string? SystemPromptTemplate { get; set; }
    public int? MaxOutputTokens { get; set; }
    public bool IsActive { get; set; } = true;
}
