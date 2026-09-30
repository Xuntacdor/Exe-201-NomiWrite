using NomiWrite.Payment.Application.DTOs;

namespace NomiWrite.Payment.Application.Interfaces;

public enum SePayWebhookOutcome
{
    /// <summary>The order was settled and PaymentCompletedEvent was published.</summary>
    Completed,

    /// <summary>This transfer (or this order) had already been settled by an earlier delivery.</summary>
    Duplicate,

    /// <summary>The transfer does not belong to a fulfillable order. Never retried.</summary>
    Ignored
}

public sealed record SePayWebhookResult(
    SePayWebhookOutcome Outcome,
    string? OrderReference,
    string? Reason);

public interface ISePayWebhookService
{
    /// <summary>
    /// Settles the order referenced by a SePay transfer.
    /// <paramref name="expectedAccountNumber"/> is the account this merchant collects into;
    /// transfers landing on any other account are ignored. Pass null to skip that check.
    /// </summary>
    Task<SePayWebhookResult> ProcessAsync(SePayWebhookDto payload, string? expectedAccountNumber);
}
