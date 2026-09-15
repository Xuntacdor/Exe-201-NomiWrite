using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using NomiWrite.Logging.Domain.Entities;
using NomiWrite.Logging.Infrastructure.Options;

namespace NomiWrite.Logging.Infrastructure.Persistence;

/// <summary>
/// Owns the connection to the MongoDB database and exposes typed collections.
/// Also responsible for creating the TTL + secondary indexes used by activity log queries.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;
    private readonly ILogger<MongoDbContext> _logger;

    public IMongoCollection<UserActivityLog> ActivityLogs { get; }

    public MongoDbContext(
        MongoDbSettings settings,
        ILogger<MongoDbContext> logger)
    {
        _settings = settings;
        _logger = logger;

        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
        ActivityLogs = _database.GetCollection<UserActivityLog>(GetCollectionName(typeof(UserActivityLog)));
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        var models = new List<CreateIndexModel<UserActivityLog>>
        {
            // TTL index: documents expire 180 days after their Timestamp.
            new CreateIndexModel<UserActivityLog>(
                Builders<UserActivityLog>.IndexKeys.Ascending(x => x.Timestamp),
                new CreateIndexOptions
                {
                    Name = "IX_ttl_timestamp",
                    ExpireAfter = TimeSpan.FromDays(_settings.TtlExpireAfterDays)
                }),

            new CreateIndexModel<UserActivityLog>(
                Builders<UserActivityLog>.IndexKeys.Ascending(x => x.UserId).Descending(x => x.Timestamp),
                new CreateIndexOptions { Name = "IX_userId_timestamp" }),

            new CreateIndexModel<UserActivityLog>(
                Builders<UserActivityLog>.IndexKeys.Ascending(x => x.Action).Descending(x => x.Timestamp),
                new CreateIndexOptions { Name = "IX_action_timestamp" }),

            new CreateIndexModel<UserActivityLog>(
                Builders<UserActivityLog>.IndexKeys.Descending(x => x.Timestamp),
                new CreateIndexOptions { Name = "IX_timestamp_desc" })
        };

        await ActivityLogs.Indexes.CreateManyAsync(models, cancellationToken: cancellationToken);
        _logger.LogInformation(
            "Ensured MongoDB indexes for collection {Collection} in database {Database}",
            "activity_logs", _settings.DatabaseName);
    }

    private static string GetCollectionName(Type documentType)
    {
        var attribute = documentType.GetCustomAttributes(typeof(BsonCollectionAttribute), inherit: false)
            .Cast<BsonCollectionAttribute>()
            .FirstOrDefault();

        return attribute?.CollectionName
            ?? throw new InvalidOperationException(
                $"Document type {documentType.Name} does not define a [BsonCollection] attribute.");
    }
}