namespace NomiWrite.Logging.Infrastructure.Options;

public class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = "NomiWriteLogDb";

    /// <summary>TTL (in days) after which activity log documents expire.</summary>
    public int TtlExpireAfterDays { get; set; } = 180;
}