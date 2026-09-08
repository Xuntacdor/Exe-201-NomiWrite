using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Exceptions;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Payment;

namespace NomiWrite.Payment.Application.Services;

public class PaymentService : IPaymentService
{
    private static readonly string DefaultCurrency = "VND";

    private readonly IPaymentDbContext _dbContext;
    private readonly IPaymentGatewayService _gatewayService;
    private readonly IValidator<CreatePaymentRequestDto> _createPaymentValidator;
    private readonly IValidator<CreateRefundRequestDto> _createRefundValidator;
    private readonly IPromoCodeValidator _promoCodeValidator;
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentService(
        IPaymentDbContext dbContext,
        IPaymentGatewayService gatewayService,
        IValidator<CreatePaymentRequestDto> createPaymentValidator,
        IValidator<CreateRefundRequestDto> createRefundValidator,
        IPromoCodeValidator promoCodeValidator,
        IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _gatewayService = gatewayService;
        _createPaymentValidator = createPaymentValidator;
        _createRefundValidator = createRefundValidator;
        _promoCodeValidator = promoCodeValidator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreatePaymentResponseDto> CreatePaymentAsync(Guid userId, CreatePaymentRequestDto request)
    {
        var validationResult = await _createPaymentValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var now = DateTime.UtcNow;

        var amount = request.Amount;
        int? appliedDiscountPercent = null;

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            // Fail open: if the Subscription service is unreachable or rejects the
            // code (including an invalid or expired code), we simply do not apply a
            // discount rather than blocking the payment from being created. Promo
            // codes here are informational discounts, not strictly enforced
            // one-time-use codes.
            var promoResult = await _promoCodeValidator.ValidateAsync(request.PromoCode);

            if (promoResult.Valid && promoResult.DiscountPercent is > 0 and <= 100)
            {
                appliedDiscountPercent = promoResult.DiscountPercent.Value;
                amount = decimal.Round(amount * (100m - appliedDiscountPercent.Value) / 100m, 2);
            }
        }

        var payment = new PaymentOrder
        {
            UserId = userId,
            Amount = amount,
            Currency = NormalizeCurrency(request.Currency),
            Provider = request.Provider,
            Status = PaymentStatus.Pending,
            OrderReference = GenerateOrderReference(),
            PlanId = request.PlanId,
            AppliedDiscountPercent = appliedDiscountPercent,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        string? paymentUrl = null;

        // TODO: Integrate the real payment gateway SDK (VNPay / MoMo / VietQR) for
        // the selected provider. Each provider SDK requires merchant credentials
        // (partner code, merchant account, secret/hash secret) loaded from
        // configuration and must return the hosted checkout URL used to redirect
        // the buyer. Replace this call with the concrete gateway implementation.
        var gatewayResult = await _gatewayService.CreatePaymentAsync(payment);
        paymentUrl = gatewayResult.PaymentUrl;

        await _publishEndpoint.Publish(new PaymentCreatedEvent
        {
            PaymentId = payment.Id,
            UserId = payment.UserId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Provider = payment.Provider.ToString(),
            CreatedAt = now,
            PlanId = payment.PlanId
        });

        return new CreatePaymentResponseDto
        {
            PaymentId = payment.Id,
            OrderReference = payment.OrderReference,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Provider = payment.Provider,
            Status = payment.Status,
            PaymentUrl = paymentUrl,
            CreatedAt = payment.CreatedAt,
            AppliedDiscountPercent = payment.AppliedDiscountPercent
        };
    }

    public async Task<PaymentStatusResponseDto> HandleWebhookAsync(WebhookCallbackDto callback)
    {
        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.OrderReference == callback.OrderReference)
            ?? throw new PaymentNotFoundException(callback.OrderReference);

        if (callback.Provider != payment.Provider)
        {
            throw new InvalidWebhookException(
                $"Provider '{callback.Provider}' does not match payment '{payment.OrderReference}' provider '{payment.Provider}'.");
        }

        // TODO: Verify the webhook signature before trusting the payload. VNPay
        // signs with an HMAC-SHA512 checksum, MoMo includes a signature field and
        // VietQR relies on the provider settlement report. The concrete gateway
        // implementation must validate the signature using the provider's shared
        // secret credentials loaded from configuration.
        var verification = await _gatewayService.VerifyWebhookAsync(callback);
        if (!verification.IsValid)
        {
            throw new InvalidWebhookException(verification.Reason ?? "Webhook signature verification failed.");
        }

        var now = DateTime.UtcNow;
        _dbContext.PaymentTransactions.Add(new PaymentTransaction
        {
            PaymentId = payment.Id,
            ProviderTransactionId = callback.ProviderTransactionId,
            RawPayload = callback.RawPayload,
            ReceivedAt = callback.ReceivedAt == default ? now : callback.ReceivedAt
        });

        PaymentCompletedEvent? completed = null;
        PaymentFailedEvent? failed = null;

        if (callback.IsSuccess && payment.Status != PaymentStatus.Completed)
        {
            payment.Status = PaymentStatus.Completed;
            payment.UpdatedAt = now;
            completed = new PaymentCompletedEvent(payment.Id, payment.UserId, payment.Amount, payment.OrderReference, payment.PlanId);
        }
        else if (!callback.IsSuccess && payment.Status != PaymentStatus.Failed)
        {
            payment.Status = PaymentStatus.Failed;
            payment.UpdatedAt = now;
            failed = new PaymentFailedEvent(
                payment.Id,
                payment.UserId,
                callback.Reason ?? "Payment failed at the payment provider.");
        }

        await _dbContext.SaveChangesAsync();

        if (completed is not null)
        {
            await _publishEndpoint.Publish(completed);
        }
        else if (failed is not null)
        {
            await _publishEndpoint.Publish(failed);
        }

        return ToStatusResponse(payment);
    }

    public async Task<PaymentStatusResponseDto> GetPaymentStatusAsync(Guid paymentId)
    {
        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new PaymentNotFoundException(paymentId);

        return ToStatusResponse(payment);
    }

    public async Task<IEnumerable<PaymentHistoryItemDto>> GetPaymentHistoryAsync(Guid userId)
    {
        return await _dbContext.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentHistoryItemDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Currency = p.Currency,
                Provider = p.Provider,
                Status = p.Status,
                PlanId = p.PlanId,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<RefundRequestDto> CreateRefundRequestAsync(Guid userId, Guid paymentOrderId, string reason)
    {
        var refundDto = new CreateRefundRequestDto { Reason = reason };
        var validationResult = await _createRefundValidator.ValidateAsync(refundDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentOrderId)
            ?? throw new PaymentNotFoundException(paymentOrderId);

        if (payment.UserId != userId)
            throw new InvalidRefundException("Payment does not belong to the caller.", 403);

        if (payment.Status != PaymentStatus.Completed)
            throw new InvalidRefundException("Only completed payments can be refunded.");

        var now = DateTime.UtcNow;
        var refundRequest = new RefundRequest
        {
            PaymentOrderId = paymentOrderId,
            UserId = userId,
            Reason = reason,
            Status = RefundStatus.Pending,
            RequestedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.RefundRequests.Add(refundRequest);
        await _dbContext.SaveChangesAsync();

        return ToRefundRequestDto(refundRequest);
    }

    public async Task<IEnumerable<RefundRequestDto>> GetRefundRequestsAsync(Guid userId)
    {
        return await _dbContext.RefundRequests
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new RefundRequestDto
            {
                Id = r.Id,
                PaymentOrderId = r.PaymentOrderId,
                Reason = r.Reason,
                Status = r.Status,
                RequestedAt = r.RequestedAt,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    private static RefundRequestDto ToRefundRequestDto(RefundRequest refundRequest)
    {
        return new RefundRequestDto
        {
            Id = refundRequest.Id,
            PaymentOrderId = refundRequest.PaymentOrderId,
            Reason = refundRequest.Reason,
            Status = refundRequest.Status,
            RequestedAt = refundRequest.RequestedAt,
            CreatedAt = refundRequest.CreatedAt
        };
    }

    private static PaymentStatusResponseDto ToStatusResponse(PaymentOrder payment)
    {
        return new PaymentStatusResponseDto
        {
            PaymentId = payment.Id,
            OrderReference = payment.OrderReference,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Provider = payment.Provider,
            Status = payment.Status,
            UpdatedAt = payment.UpdatedAt
        };
    }

    private static string NormalizeCurrency(string currency)
    {
        return string.IsNullOrWhiteSpace(currency) ? DefaultCurrency : currency.Trim().ToUpperInvariant();
    }

    private static string GenerateOrderReference()
    {
        return $"PAY-{Guid.NewGuid():N}".ToUpperInvariant();
    }
}