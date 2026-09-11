using NomiWrite.Notification.Application.DTOs;

namespace NomiWrite.Notification.Application.Interfaces;

public interface INotificationService
{
    Task<PagedResultDto<NotificationDto>> GetNotificationsAsync(Guid userId, bool? unreadOnly, int page, int pageSize);
    Task MarkAsReadAsync(Guid userId, Guid notificationId);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task<NotificationPreferenceDto> GetPreferencesAsync(Guid userId);
    Task<NotificationPreferenceDto> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequestDto dto);
}
