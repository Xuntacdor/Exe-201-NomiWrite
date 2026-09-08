using NomiWrite.Payment.Domain.Common;
using NomiWrite.Payment.Domain.Enums;

namespace NomiWrite.Payment.Domain.Entities;

public class RefundRequest : BaseEntity
{
    public Guid PaymentOrderId { get; set; }
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    public DateTime RequestedAt { get; set; }

    public PaymentOrder? PaymentOrder { get; set; }
}
