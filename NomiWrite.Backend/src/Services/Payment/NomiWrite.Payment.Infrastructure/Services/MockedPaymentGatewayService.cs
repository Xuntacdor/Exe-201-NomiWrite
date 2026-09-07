using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Domain.Entities;

namespace NomiWrite.Payment.Infrastructure.Services;

public class MockedPaymentGatewayService : IPaymentGatewayService
{
    private const string MockCheckoutBaseUrl = "https://checkout.payment.local";

    public Task<GatewayCreatePaymentResult> CreatePaymentAsync(PaymentOrder payment)
    {
        // TODO: Replace with the real payment gateway SDK for the selected provider.
        // VNPay / MoMo / VietQR require merchant credentials (partner code, merchant
        // account, secret/hash secret) loaded from configuration. Each SDK returns
        // a provider transaction id and a hosted checkout URL used to redirect the
        // buyer. This stub mocks both values so the payment flow can be exercised
        // end-to-end until the vendor SDKs are integrated.
        var providerTransactionId = $"MOCK-{payment.OrderReference}";
        var paymentUrl = $"{MockCheckoutBaseUrl}/{payment.Provider.ToString().ToLowerInvariant()}/{payment.OrderReference}";

        return Task.FromResult(new GatewayCreatePaymentResult(providerTransactionId, paymentUrl));
    }

    public Task<GatewayWebhookVerificationResult> VerifyWebhookAsync(WebhookCallbackDto callback)
    {
        // TODO: Replace with real webhook signature verification per provider.
        // VNPay signs with an HMAC-SHA512 checksum, MoMo includes a signature field
        // and VietQR relies on the provider settlement report. Authenticity must be
        // validated using the provider's shared secret credentials. This stub always
        // accepts the payload so the webhook flow can be exercised end-to-end.
        return Task.FromResult(new GatewayWebhookVerificationResult(true, null));
    }
}