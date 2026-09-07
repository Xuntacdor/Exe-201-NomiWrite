using NomiWrite.Payment.Domain.Common;
using NomiWrite.Payment.Domain.Enums;

namespace NomiWrite.Payment.Domain.Entities;

public class PaymentOrder : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public PaymentProvider Provider { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string OrderReference { get; set; } = string.Empty;

    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
}