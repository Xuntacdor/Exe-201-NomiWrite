using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NomiWrite.Subscription.Infrastructure.Persistence;

public class SubscriptionDbContextFactory : IDesignTimeDbContextFactory<SubscriptionDbContext>
{
    public SubscriptionDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SubscriptionDb")
            ?? "Host=localhost;Database=nomiwrite_subscription;Username=postgres";
        var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new SubscriptionDbContext(options);
    }
}
