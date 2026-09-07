using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Domain.Entities;

namespace NomiWrite.Payment.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<GatewayCreatePaymentResult> CreatePaymentAsync(PaymentOrder payment);

    Task<GatewayWebhookVerificationResult> VerifyWebhookAsync(WebhookCallbackDto callback);
}

public sealed record GatewayCreatePaymentResult(string? ProviderTransactionId, string? PaymentUrl);

public sealed record GatewayWebhookVerificationResult(bool IsValid, string? Reason);