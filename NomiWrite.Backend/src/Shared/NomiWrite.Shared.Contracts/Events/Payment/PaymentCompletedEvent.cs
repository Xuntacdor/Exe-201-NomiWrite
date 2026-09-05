namespace NomiWrite.Shared.Contracts.Events.Payment;

public sealed record PaymentCompletedEvent(Guid PaymentId, Guid UserId, decimal Amount, string OrderReference);