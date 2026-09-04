namespace NomiWrite.Shared.Contracts.Events.Payment;

/// <summary>
/// Published by Payment Service when a payment is successfully completed.
/// Consumed by User Service (unlock premium), Notification, etc.
/// </summary>
public record PaymentCompletedEvent
{
    public Guid PaymentId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}
