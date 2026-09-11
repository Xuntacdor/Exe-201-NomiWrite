using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Notification.Domain.Entities;

namespace NomiWrite.Notification.Infrastructure.Persistence.Configurations;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasDatabaseName("ix_notification_preferences_user_id");

        builder.Property(p => p.EmailNotificationsEnabled)
            .HasColumnName("email_notifications_enabled")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.InAppNotificationsEnabled)
            .HasColumnName("in_app_notifications_enabled")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.GradingAlerts)
            .HasColumnName("grading_alerts")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.MarketingAlerts)
            .HasColumnName("marketing_alerts")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
