using MongoDB.Bson;
using MongoDB.Driver;
using NomiWrite.Logging.Application.DTOs;
using NomiWrite.Logging.Application.Interfaces;
using NomiWrite.Logging.Domain.Entities;
using ApplicationSortDirection = NomiWrite.Logging.Application.DTOs.SortDirection;

namespace NomiWrite.Logging.Infrastructure.Persistence;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly IMongoCollection<UserActivityLog> _collection;

    public ActivityLogRepository(MongoDbContext dbContext)
        => _collection = dbContext.ActivityLogs;

    public async Task AddAsync(UserActivityLog log, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(log.Id))
            log.Id = ObjectId.GenerateNewId().ToString();

        await _collection.InsertOneAsync(log, null, cancellationToken);
    }

    public async Task<PagedResultDto<ActivityLogItemDto>> GetPagedAsync(
        ActivityLogQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(query);
        var sort = BuildSort(query);

        var totalCount = await _collection.CountDocumentsAsync(filter, null, cancellationToken);

        var items = await _collection
            .Find(filter)
            .Sort(sort)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ActivityLogItemDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = (int)totalCount,
            Page = query.PageIndex,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    private static FilterDefinition<UserActivityLog> BuildFilter(ActivityLogQueryDto query)
    {
        var builder = Builders<UserActivityLog>.Filter;
        var filters = new List<FilterDefinition<UserActivityLog>>();

        if (!string.IsNullOrWhiteSpace(query.UserId))
            filters.Add(builder.Eq(x => x.UserId, query.UserId));

        if (!string.IsNullOrWhiteSpace(query.Action))
            filters.Add(builder.Eq(x => x.Action, query.Action));

        if (query.FromDate.HasValue)
            filters.Add(builder.Gte(x => x.Timestamp, query.FromDate.Value));

        if (query.ToDate.HasValue)
            filters.Add(builder.Lte(x => x.Timestamp, query.ToDate.Value));

        return filters.Count == 0 ? builder.Empty : builder.And(filters);
    }

    private static SortDefinition<UserActivityLog> BuildSort(ActivityLogQueryDto query)
    {
        var builder = Builders<UserActivityLog>.Sort;

        var fieldName = query.SortBy?.ToLowerInvariant() switch
        {
            "userId" => "userId",
            "action" => "action",
            "serviceName" => "serviceName",
            _ => "timestamp"
        };

        return query.SortDirection == ApplicationSortDirection.Desc
            ? builder.Descending(fieldName)
            : builder.Ascending(fieldName);
    }

    private static ActivityLogItemDto ToDto(UserActivityLog log)
    {
        return new ActivityLogItemDto
        {
            Id = log.Id,
            UserId = log.UserId,
            Action = log.Action,
            ServiceName = log.ServiceName,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            Metadata = ToMetadataDictionary(log.Metadata),
            Timestamp = log.Timestamp
        };
    }

    private static Dictionary<string, object>? ToMetadataDictionary(BsonDocument? metadata)
    {
        if (metadata is null)
            return null;

        var result = new Dictionary<string, object>();
        foreach (var element in metadata)
            result[element.Name] = BsonValueToDotNet(element.Value);

        return result;
    }

    private static object BsonValueToDotNet(BsonValue value)
    {
        return value.BsonType switch
        {
            BsonType.String => value.AsString,
            BsonType.Int32 => value.AsInt32,
            BsonType.Int64 => value.AsInt64,
            BsonType.Double => value.AsDouble,
            BsonType.Decimal128 => Decimal128.ToDecimal(value.AsDecimal128),
            BsonType.Boolean => value.AsBoolean,
            BsonType.DateTime => value.ToUniversalTime(),
            BsonType.ObjectId => value.AsObjectId.ToString(),
            BsonType.Document => ToMetadataDictionary(value.AsBsonDocument) ?? new Dictionary<string, object>(),
            BsonType.Array => value.AsBsonArray.Select(BsonValueToDotNet).ToList(),
            _ => value.ToString() ?? string.Empty
        };
    }
}