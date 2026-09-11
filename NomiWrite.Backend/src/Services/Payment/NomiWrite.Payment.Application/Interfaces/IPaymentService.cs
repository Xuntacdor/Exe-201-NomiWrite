using NomiWrite.Payment.Application.DTOs;

namespace NomiWrite.Payment.Application.Interfaces;

public interface IPaymentService
{
    Task<CreatePaymentResponseDto> CreatePaymentAsync(Guid userId, CreatePaymentRequestDto request);

    Task<PaymentStatusResponseDto> HandleWebhookAsync(WebhookCallbackDto callback);

    Task<PaymentStatusResponseDto> GetPaymentStatusAsync(Guid userId, Guid paymentId);

    Task<IEnumerable<PaymentHistoryItemDto>> GetPaymentHistoryAsync(Guid userId);

    Task<RefundRequestDto> CreateRefundRequestAsync(Guid userId, Guid paymentOrderId, string reason);

    Task<IEnumerable<RefundRequestDto>> GetRefundRequestsAsync(Guid userId);
}
