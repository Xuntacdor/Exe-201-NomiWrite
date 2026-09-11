using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Subscription.Domain.Entities;
using NomiWrite.Subscription.Domain.Enums;

namespace NomiWrite.Subscription.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    private static readonly DateTime SeedCreatedAt = new(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.Price)
            .HasColumnName("price")
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.Currency)
            .HasColumnName("currency")
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("VND");

        builder.Property(p => p.BillingCycle)
            .HasColumnName("billing_cycle")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.DurationDays)
            .HasColumnName("duration_days")
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.FeaturesJson)
            .HasColumnName("features_json")
            .HasColumnType("jsonb")
            .HasDefaultValue("[]");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasData(
            new SubscriptionPlan
            {
                Id = new Guid("11111111-1111-1111-1111-111111111101"),
                Name = "VIP Monthly",
                Description = "Monthly VIP subscription for NomiWrite premium features.",
                Price = 99000,
                Currency = "VND",
                BillingCycle = BillingCycle.Monthly,
                DurationDays = 30,
                IsActive = true,
                FeaturesJson = "[\"VIP Sample Answers\",\"Priority Support\"]",
                CreatedAt = SeedCreatedAt
            },
            new SubscriptionPlan
            {
                Id = new Guid("22222222-2222-2222-2222-222222222201"),
                Name = "VIP Yearly",
                Description = "Yearly VIP subscription for NomiWrite premium features.",
                Price = 990000,
                Currency = "VND",
                BillingCycle = BillingCycle.Yearly,
                DurationDays = 365,
                IsActive = true,
                FeaturesJson = "[\"VIP Sample Answers\",\"Priority Support\"]",
                CreatedAt = SeedCreatedAt
            });
    }
}
