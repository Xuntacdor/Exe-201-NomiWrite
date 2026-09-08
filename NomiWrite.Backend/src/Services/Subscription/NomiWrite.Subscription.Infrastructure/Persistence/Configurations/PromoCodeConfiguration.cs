using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Subscription.Domain.Entities;

namespace NomiWrite.Subscription.Infrastructure.Persistence.Configurations;

public class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    private static readonly DateTime SeedCreatedAt = new(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.ToTable("promo_codes");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(p => p.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("ix_promo_codes_code");

        builder.Property(p => p.DiscountPercent)
            .HasColumnName("discount_percent")
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(p => p.MaxRedemptions)
            .HasColumnName("max_redemptions");

        builder.Property(p => p.TimesRedeemed)
            .HasColumnName("times_redeemed")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasData(
            new PromoCode
            {
                Id = new Guid("33333333-3333-3333-3333-333333333301"),
                Code = "WELCOME20",
                DiscountPercent = 20,
                IsActive = true,
                ExpiresAt = null,
                MaxRedemptions = null,
                TimesRedeemed = 0,
                CreatedAt = SeedCreatedAt
            });
    }
}
