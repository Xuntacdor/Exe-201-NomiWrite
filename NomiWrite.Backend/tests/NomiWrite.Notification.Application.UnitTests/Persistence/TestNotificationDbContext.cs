using Microsoft.EntityFrameworkCore;
using NomiWrite.Notification.Infrastructure.Persistence;

namespace NomiWrite.Notification.Application.UnitTests.Persistence;

public static class TestNotificationDbContext
{
    public static NotificationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NotificationDbContext(options);
    }
}