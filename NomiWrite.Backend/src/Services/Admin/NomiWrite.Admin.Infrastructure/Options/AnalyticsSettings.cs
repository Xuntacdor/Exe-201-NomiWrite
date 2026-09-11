namespace NomiWrite.Admin.Infrastructure.Options;

public class AnalyticsSettings
{
    public const string SectionName = "AnalyticsSettings";

    public int CacheTtlMinutes { get; set; } = 10;
    public int TimeoutSeconds { get; set; } = 5;
}