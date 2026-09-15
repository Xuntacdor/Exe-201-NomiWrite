using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using NomiWrite.Logging.Application.Interfaces;
using NomiWrite.Logging.Domain.Entities;
using NomiWrite.Shared.Contracts.Events.Logging;

namespace NomiWrite.Logging.Infrastructure.Consumers;

/// <summary>
/// Persists user activity events published by other services into MongoDB.
/// </summary>
public class UserActivityLoggedConsumer : IConsumer<UserActivityLoggedEvent>
{
    private readonly IActivityLogRepository _repository;
    private readonly ILogger<UserActivityLoggedConsumer> _logger;

    public UserActivityLoggedConsumer(
        IActivityLogRepository repository,
        ILogger<UserActivityLoggedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserActivityLoggedEvent> context)
    {
        var message = context.Message;

        var log = new UserActivityLog
        {
            UserId = message.UserId,
            Action = message.Action,
            ServiceName = message.ServiceName,
            IpAddress = message.IpAddress,
            UserAgent = message.UserAgent,
            Metadata = ToBsonDocument(message.Metadata),
            Timestamp = message.Timestamp == default ? DateTime.UtcNow : message.Timestamp
        };

        await _repository.AddAsync(log, context.CancellationToken);

        _logger.LogInformation(
            "Persisted UserActivityLoggedEvent for user {UserId} | action {Action} | service {ServiceName}",
            message.UserId, message.Action, message.ServiceName);
    }

    private static BsonDocument? ToBsonDocument(Dictionary<string, object>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
            return null;

        var document = new BsonDocument();
        foreach (var (key, value) in metadata)
            document[key] = ToBsonValue(value);

        return document;
    }

    private static BsonValue ToBsonValue(object value)
    {
        return value switch
        {
            BsonValue bson => bson,
            string s => new BsonString(s),
            bool b => new BsonBoolean(b),
            int i => new BsonInt32(i),
            long l => new BsonInt64(l),
            double d => new BsonDouble(d),
            decimal m => BsonDecimal128.Create(m),
            DateTime dt => new BsonDateTime(dt),
            Dictionary<string, object> dict => ToBsonDocument(dict) ?? new BsonDocument(),
            IEnumerable<object> enumerable => new BsonArray(enumerable.Select(ToBsonValue)),
            Guid guid => new BsonString(guid.ToString()),
            null => BsonNull.Value,
            _ => new BsonString(value.ToString() ?? string.Empty)
        };
    }
}