using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NomiWrite.Logging.Domain.Entities;

/// <summary>
/// Represents a single user activity log entry stored in MongoDB.
/// The collection is named "activity_logs" and has a TTL index on Timestamp
/// (documents expire after 180 days).
/// </summary>
[BsonCollection("activity_logs")]
public class UserActivityLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;

    [BsonElement("serviceName")]
    public string ServiceName { get; set; } = string.Empty;

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("metadata")]
    public BsonDocument? Metadata { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Attribute used to specify the MongoDB collection name for a document class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class BsonCollectionAttribute : Attribute
{
    public string CollectionName { get; }

    public BsonCollectionAttribute(string collectionName)
    {
        CollectionName = collectionName;
    }
}
