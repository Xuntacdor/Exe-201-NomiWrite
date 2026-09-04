namespace NomiWrite.Shared.Contracts.Events.Payment;

/// <summary>
/// Published by Payment Service when a payment fails.
/// Consumed by Notification service, Logging, etc.
/// </summary>
public record PaymentFailedEvent
{
    public Guid PaymentId { get; init; }
    public Guid UserId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; } = DateTime.UtcNow;
}
