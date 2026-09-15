using Microsoft.EntityFrameworkCore;
using NomiWrite.Subscription.Infrastructure.Persistence;

namespace NomiWrite.Subscription.Application.UnitTests.Persistence;

public static class TestSubscriptionDbContext
{
    public static SubscriptionDbContext Create()
    {
        var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SubscriptionDbContext(options);
    }
}