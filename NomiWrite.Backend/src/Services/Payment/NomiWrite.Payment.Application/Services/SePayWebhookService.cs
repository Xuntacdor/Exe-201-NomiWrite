using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MassTransit;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Payment;

namespace NomiWrite.Payment.Application.Services;

public sealed class SePayWebhookService : ISePayWebhookService
{
    private const string TransferTypeIn = "in";

    private readonly IPaymentDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SePayWebhookService> _logger;

    public SePayWebhookService(
        IPaymentDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ILogger<SePayWebhookService> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<SePayWebhookResult> ProcessAsync(SePayWebhookDto payload, string? expectedAccountNumber)
    {
        // Only inbound transfers can ever settle an order. Without this an
        // outgoing transfer that happened to mention an order id would credit it.
        if (!string.Equals(payload.TransferType, TransferTypeIn, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "SePay transfer {SePayId} ignored: transferType '{TransferType}' is not 'in'.",
                payload.Id,
                payload.TransferType);
            return Ignored(null, "transfer_type");
        }

        if (payload.TransferAmount <= 0)
        {
            _logger.LogWarning("SePay transfer {SePayId} ignored: non-positive amount {Amount}.", payload.Id, payload.TransferAmount);
            return Ignored(null, "non_positive_amount");
        }

        if (!string.IsNullOrWhiteSpace(expectedAccountNumber)
            && !string.Equals(payload.AccountNumber?.Trim(), expectedAccountNumber.Trim(), StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "SePay transfer {SePayId} ignored: landed on account {ActualAccount}, expected {ExpectedAccount}.",
                payload.Id,
                payload.AccountNumber,
                expectedAccountNumber);
            return Ignored(null, "unexpected_account");
        }

        // SePay replays a transfer on retry and on manual replay from the webhook
        // dashboard. Its `id` never changes, so it is the dedup key.
        var transactionId = payload.Id.ToString(CultureInfo.InvariantCulture);

        if (await _dbContext.PaymentTransactions
                .AnyAsync(t => t.ProviderTransactionId == transactionId))
        {
            _logger.LogInformation("SePay transfer {SePayId} already recorded; treating as duplicate.", payload.Id);
            return new SePayWebhookResult(SePayWebhookOutcome.Duplicate, null, "transaction_already_recorded");
        }

        var candidates = SePayOrderReference.ExtractCandidates(payload);
        if (candidates.Count == 0)
        {
            _logger.LogWarning(
                "SePay transfer {SePayId} ({BankRef}) of {Amount} VND carries no NomiWrite order reference. Content: '{Content}'.",
                payload.Id,
                payload.ReferenceCode,
                payload.TransferAmount,
                payload.Content);
            return Ignored(null, "no_order_reference");
        }

        var payment = await FindOrderAsync(candidates);
        if (payment is null)
        {
            _logger.LogWarning(
                "SePay transfer {SePayId} ({BankRef}) of {Amount} VND references unknown order(s) {Candidates}. Content: '{Content}'.",
                payload.Id,
                payload.ReferenceCode,
                payload.TransferAmount,
                string.Join(", ", candidates),
                payload.Content);
            return Ignored(null, "order_not_found");
        }

        if (payment.Provider != PaymentProvider.VietQR)
        {
            _logger.LogWarning(
                "SePay transfer {SePayId} references order {OrderReference}, which belongs to provider {Provider}.",
                payload.Id,
                payment.OrderReference,
                payment.Provider);
            return Ignored(payment.OrderReference, "provider_mismatch");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return new SePayWebhookResult(SePayWebhookOutcome.Duplicate, payment.OrderReference, $"order_already_{payment.Status}");
        }

        // ix_payment_transactions_payment_id is unique, so an order can hold at
        // most one transfer. This is the second layer behind the check above.
        if (await _dbContext.PaymentTransactions.AnyAsync(t => t.PaymentId == payment.Id))
        {
            return new SePayWebhookResult(SePayWebhookOutcome.Duplicate, payment.OrderReference, "transaction_already_recorded");
        }

        if (payload.TransferAmount < payment.Amount)
        {
            _logger.LogWarning(
                "SePay transfer {SePayId} underpaid order {OrderReference}: sent {Transferred}, order total {Expected}.",
                payload.Id,
                payment.OrderReference,
                payload.TransferAmount,
                payment.Amount);
            return Ignored(payment.OrderReference, "amount_below_order_total");
        }

        var now = DateTime.UtcNow;
        var overpaid = payload.TransferAmount - payment.Amount;
        if (overpaid > 0)
        {
            _logger.LogWarning(
                "SePay transfer {SePayId} overpaid order {OrderReference} by {Overpaid} VND ({Transferred} sent, {Expected} owed). Fulfil as paid and reconcile the surplus manually.",
                payload.Id,
                payment.OrderReference,
                overpaid,
                payload.TransferAmount,
                payment.Amount);
        }

        payment.Status = PaymentStatus.Completed;
        payment.UpdatedAt = now;

        _dbContext.PaymentTransactions.Add(new PaymentTransaction
        {
            PaymentId = payment.Id,
            ProviderTransactionId = transactionId,
            RawPayload = JsonSerializer.Serialize(payload),
            ReceivedAt = now
        });

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            // A concurrent delivery of the same transfer won the race and violated
            // the unique index. The losing request must not fulfil or publish.
            _logger.LogInformation(
                exception,
                "SePay transfer {SePayId} lost a concurrent fulfilment race for order {OrderReference}; treating as duplicate.",
                payload.Id,
                payment.OrderReference);
            return new SePayWebhookResult(SePayWebhookOutcome.Duplicate, payment.OrderReference, "concurrent_duplicate");
        }

        // Reuse the existing activation path: PaymentCompletedEvent is what the
        // Subscription service already consumes to grant or extend entitlements.
        await _publishEndpoint.Publish(new PaymentCompletedEvent(
            payment.Id,
            payment.UserId,
            payment.Amount,
            payment.OrderReference,
            payment.PlanId,
            payment.AppliedPromoCode));

        _logger.LogInformation(
            "SePay transfer {SePayId} settled order {OrderReference} for user {UserId}.",
            payload.Id,
            payment.OrderReference,
            payment.UserId);

        return new SePayWebhookResult(SePayWebhookOutcome.Completed, payment.OrderReference, null);
    }

    private async Task<PaymentOrder?> FindOrderAsync(IReadOnlyList<string> candidates)
    {
        foreach (var candidate in candidates)
        {
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.OrderReference == candidate);

            if (payment is not null)
                return payment;
        }

        return null;
    }

    private static SePayWebhookResult Ignored(string? orderReference, string reason) =>
        new(SePayWebhookOutcome.Ignored, orderReference, reason);
}
