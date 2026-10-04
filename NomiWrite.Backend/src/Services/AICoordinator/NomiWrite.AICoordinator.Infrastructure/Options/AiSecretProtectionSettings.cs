namespace NomiWrite.AICoordinator.Infrastructure.Options;

public sealed class AiSecretProtectionSettings
{
    public const string SectionName = "AiSecretProtection";

    // Base64-encoded 32-byte key. Configure through environment or a secret store.
    public string MasterKey { get; set; } = string.Empty;
}
