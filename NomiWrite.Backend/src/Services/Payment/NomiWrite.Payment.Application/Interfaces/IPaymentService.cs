using NomiWrite.Payment.Application.DTOs;

namespace NomiWrite.Payment.Application.Interfaces;

public interface IPaymentService
{
    Task<CreatePaymentResponseDto> CreatePaymentAsync(Guid userId, CreatePaymentRequestDto request);

    Task<PaymentStatusResponseDto> HandleWebhookAsync(WebhookCallbackDto callback);

    Task<PaymentStatusResponseDto> GetPaymentStatusAsync(Guid paymentId);
}