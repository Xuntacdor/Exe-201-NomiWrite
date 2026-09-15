using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NomiWrite.Logging.Infrastructure.Persistence;

namespace NomiWrite.Logging.Infrastructure.Services;

/// <summary>
/// Creates the MongoDB TTL/secondary indexes once at startup without blocking
/// the web server from accepting traffic.
/// </summary>
public class MongoIndexInitializer : BackgroundService
{
    private readonly MongoDbContext _dbContext;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(MongoDbContext dbContext, ILogger<MongoIndexInitializer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _dbContext.EnsureIndexesAsync(stoppingToken);
        }
        catch (Exception exception)
        {
            // A missing index must not take down the service; searches still work,
            // they are just less efficient until the next restart retries.
            _logger.LogError(exception, "Failed to initialize MongoDB indexes for activity logs");
        }
    }
}