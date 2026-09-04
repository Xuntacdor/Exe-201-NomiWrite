namespace NomiWrite.Shared.Contracts.Events.Payment;

/// <summary>
/// Published by Payment Service when a payment request is created.
/// </summary>
public record PaymentCreatedEvent
{
    public Guid PaymentId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "VND";
    public string Provider { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
