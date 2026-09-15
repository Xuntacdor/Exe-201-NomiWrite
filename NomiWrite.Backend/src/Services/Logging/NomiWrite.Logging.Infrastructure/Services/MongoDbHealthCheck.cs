using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using NomiWrite.Logging.Infrastructure.Persistence;

namespace NomiWrite.Logging.Infrastructure.Services;

/// <summary>
/// Health check that pings MongoDB and reports the result.
/// </summary>
public class MongoDbHealthCheck : IHealthCheck
{
    private readonly MongoDbContext _dbContext;

    public MongoDbHealthCheck(MongoDbContext dbContext) => _dbContext = dbContext;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            await _dbContext.ActivityLogs.Database.RunCommandAsync(
                new BsonDocumentCommand<BsonDocument>(
                    new BsonDocument("ping", 1),
                    BsonDocumentSerializer.Instance),
                cancellationToken: cancellationToken);
            stopwatch.Stop();

            return HealthCheckResult.Healthy(
                $"MongoDB ping responded in {stopwatch.ElapsedMilliseconds} ms.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("MongoDB ping failed.", exception);
        }
    }
}