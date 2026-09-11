using Microsoft.EntityFrameworkCore;
using NomiWrite.Notification.Application.DTOs;
using NomiWrite.Notification.Application.Interfaces;

namespace NomiWrite.Notification.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationDbContext _dbContext;

    public NotificationService(INotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<NotificationDto>> GetNotificationsAsync(Guid userId, bool? unreadOnly, int page, int pageSize)
    {
        var query = _dbContext.Notifications
            .Where(n => n.UserId == userId || n.UserId == null);

        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                ReferenceId = n.ReferenceId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDto<NotificationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification is null)
            throw new KeyNotFoundException("Notification not found.");

        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unreadNotifications.Count == 0)
            return 0;

        var now = DateTime.UtcNow;
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync();
        return unreadNotifications.Count;
    }

    public async Task<NotificationPreferenceDto> GetPreferencesAsync(Guid userId)
    {
        var preference = await _dbContext.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preference is null)
        {
            preference = await CreateDefaultPreferencesAsync(userId);
        }

        return new NotificationPreferenceDto
        {
            Id = preference.Id,
            EmailNotificationsEnabled = preference.EmailNotificationsEnabled,
            InAppNotificationsEnabled = preference.InAppNotificationsEnabled,
            GradingAlerts = preference.GradingAlerts,
            MarketingAlerts = preference.MarketingAlerts,
            CreatedAt = preference.CreatedAt,
            UpdatedAt = preference.UpdatedAt
        };
    }

    public async Task<NotificationPreferenceDto> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequestDto dto)
    {
        var preference = await _dbContext.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preference is null)
        {
            preference = await CreateDefaultPreferencesAsync(userId);
        }

        if (dto.EmailNotificationsEnabled.HasValue)
            preference.EmailNotificationsEnabled = dto.EmailNotificationsEnabled.Value;
        if (dto.InAppNotificationsEnabled.HasValue)
            preference.InAppNotificationsEnabled = dto.InAppNotificationsEnabled.Value;
        if (dto.GradingAlerts.HasValue)
            preference.GradingAlerts = dto.GradingAlerts.Value;
        if (dto.MarketingAlerts.HasValue)
            preference.MarketingAlerts = dto.MarketingAlerts.Value;

        preference.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return new NotificationPreferenceDto
        {
            Id = preference.Id,
            EmailNotificationsEnabled = preference.EmailNotificationsEnabled,
            InAppNotificationsEnabled = preference.InAppNotificationsEnabled,
            GradingAlerts = preference.GradingAlerts,
            MarketingAlerts = preference.MarketingAlerts,
            CreatedAt = preference.CreatedAt,
            UpdatedAt = preference.UpdatedAt
        };
    }

    private async Task<Domain.Entities.NotificationPreference> CreateDefaultPreferencesAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var preference = new Domain.Entities.NotificationPreference
        {
            UserId = userId,
            EmailNotificationsEnabled = true,
            InAppNotificationsEnabled = true,
            GradingAlerts = true,
            MarketingAlerts = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.NotificationPreferences.Add(preference);
        await _dbContext.SaveChangesAsync();
        return preference;
    }
}
