using NomiWrite.Notification.Domain.Common;

namespace NomiWrite.Notification.Domain.Entities;

public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool InAppNotificationsEnabled { get; set; } = true;
    public bool GradingAlerts { get; set; } = true;
    public bool MarketingAlerts { get; set; } = true;
}
