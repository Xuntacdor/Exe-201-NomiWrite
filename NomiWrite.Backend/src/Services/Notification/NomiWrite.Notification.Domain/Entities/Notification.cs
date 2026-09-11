using NomiWrite.Notification.Domain.Common;
using NomiWrite.Notification.Domain.Enums;

namespace NomiWrite.Notification.Domain.Entities;

public class Notification : BaseEntity
{
    // Null when the notification is a system-wide broadcast (see SystemAnnouncementCreatedEventConsumer).
    public Guid? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public Guid? ReferenceId { get; set; }
    public bool IsRead { get; set; }
}
