using NomiWrite.Payment.Domain.Enums;

namespace NomiWrite.Payment.Application.DTOs;

public class CreatePaymentRequestDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public PaymentProvider Provider { get; set; }
}

public class CreatePaymentResponseDto
{
    public Guid PaymentId { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public PaymentProvider Provider { get; set; }
    public PaymentStatus Status { get; set; }
    public string? PaymentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentStatusResponseDto
{
    public Guid PaymentId { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public PaymentProvider Provider { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class WebhookCallbackDto
{
    public PaymentProvider Provider { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public string ProviderTransactionId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? Reason { get; set; }
    public string RawPayload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}