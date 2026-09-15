using NomiWrite.Payment.Domain.Enums;

namespace NomiWrite.Payment.Application.DTOs;

public class AdminPaymentQueryParamsDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public PaymentProvider? Provider { get; set; }
    public PaymentStatus? Status { get; set; }
}

public class AdminPaymentItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public PaymentProvider Provider { get; set; }
    public PaymentStatus Status { get; set; }
    public Guid? PlanId { get; set; }
    public int? AppliedDiscountPercent { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AdminPaymentListResponseDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<AdminPaymentItemDto> Items { get; set; } = new();
}