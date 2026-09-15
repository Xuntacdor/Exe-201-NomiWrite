namespace NomiWrite.Shared.Contracts.Events.Logging;

/// <summary>
/// Published by any service to record a user activity.
/// Consumed by the Logging service which persists the entry to MongoDB.
/// </summary>
public sealed record UserActivityLoggedEvent(
    string UserId,
    string Action,
    string ServiceName,
    string? IpAddress,
    string? UserAgent,
    Dictionary<string, object>? Metadata,
    DateTime Timestamp);
