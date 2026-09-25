using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Subscription.Domain.Entities;

namespace NomiWrite.Subscription.Infrastructure.Persistence.Configurations;

public class ProcessedPaymentConfiguration : IEntityTypeConfiguration<ProcessedPayment>
{
    public void Configure(EntityTypeBuilder<ProcessedPayment> builder)
    {
        builder.ToTable("processed_payments");
        builder.HasKey(p => p.PaymentOrderId);
        builder.Property(p => p.PaymentOrderId).HasColumnName("payment_order_id");
        builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(p => p.PlanId).HasColumnName("plan_id").IsRequired();
        builder.Property(p => p.ProcessedAt).HasColumnName("processed_at").IsRequired();
    }
}
