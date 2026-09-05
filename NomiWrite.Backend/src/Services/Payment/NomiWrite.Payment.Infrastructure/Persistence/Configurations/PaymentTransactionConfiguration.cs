using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Payment.Domain.Entities;

namespace NomiWrite.Payment.Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("payment_transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(t => t.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.HasIndex(t => t.PaymentId)
            .HasDatabaseName("ix_payment_transactions_payment_id");

        builder.Property(t => t.ProviderTransactionId)
            .HasColumnName("provider_transaction_id")
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(t => t.RawPayload)
            .HasColumnName("raw_payload")
            .HasColumnType("jsonb");

        builder.Property(t => t.ReceivedAt)
            .HasColumnName("received_at")
            .IsRequired();

        builder.HasOne(t => t.PaymentOrder)
            .WithMany(p => p.Transactions)
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}