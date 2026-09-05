namespace NomiWrite.Shared.Contracts.Events.Payment;

public sealed record PaymentFailedEvent(Guid PaymentId, Guid UserId, string Reason);