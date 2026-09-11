using NomiWrite.Notification.Domain.Enums;

namespace NomiWrite.Notification.Application.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public Guid? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class NotificationPreferenceDto
{
    public Guid Id { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public bool InAppNotificationsEnabled { get; set; }
    public bool GradingAlerts { get; set; }
    public bool MarketingAlerts { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdatePreferencesRequestDto
{
    public bool? EmailNotificationsEnabled { get; set; }
    public bool? InAppNotificationsEnabled { get; set; }
    public bool? GradingAlerts { get; set; }
    public bool? MarketingAlerts { get; set; }
}
