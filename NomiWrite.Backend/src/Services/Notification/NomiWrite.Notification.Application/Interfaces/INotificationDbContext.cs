using Microsoft.EntityFrameworkCore;
using NomiWrite.Notification.Domain.Entities;

namespace NomiWrite.Notification.Application.Interfaces;

public interface INotificationDbContext
{
    DbSet<Domain.Entities.Notification> Notifications { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
