using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Payment.Domain.Entities;

namespace NomiWrite.Payment.Infrastructure.Persistence.Configurations;

public class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
{
    public void Configure(EntityTypeBuilder<PaymentOrder> builder)
    {
        builder.ToTable("payment_orders");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("ix_payment_orders_user_id");

        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.Currency)
            .HasColumnName("currency")
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(p => p.Provider)
            .HasColumnName("provider")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.OrderReference)
            .HasColumnName("order_reference")
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(p => p.OrderReference)
            .IsUnique()
            .HasDatabaseName("ix_payment_orders_order_reference");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");
    }
}