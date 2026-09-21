namespace NomiWrite.Subscription.Domain.Entities;

public class ProcessedPayment
{
    public Guid PaymentOrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime ProcessedAt { get; set; }
}
