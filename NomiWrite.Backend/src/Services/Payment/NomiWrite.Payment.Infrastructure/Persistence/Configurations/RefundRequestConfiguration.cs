using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Domain.Enums;

namespace NomiWrite.Payment.Infrastructure.Persistence.Configurations;

public class RefundRequestConfiguration : IEntityTypeConfiguration<RefundRequest>
{
    public void Configure(EntityTypeBuilder<RefundRequest> builder)
    {
        builder.ToTable("refund_requests");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(r => r.PaymentOrderId)
            .HasColumnName("payment_order_id")
            .IsRequired();

        builder.HasIndex(r => r.PaymentOrderId)
            .HasDatabaseName("ix_refund_requests_payment_order_id");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("ix_refund_requests_user_id");

        builder.Property(r => r.Reason)
            .HasColumnName("reason")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.RequestedAt)
            .HasColumnName("requested_at")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(r => r.PaymentOrder)
            .WithMany(p => p.RefundRequests)
            .HasForeignKey(r => r.PaymentOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
