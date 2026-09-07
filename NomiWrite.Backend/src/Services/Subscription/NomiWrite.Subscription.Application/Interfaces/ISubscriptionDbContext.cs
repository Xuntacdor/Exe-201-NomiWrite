using Microsoft.EntityFrameworkCore;
using NomiWrite.Subscription.Domain.Entities;

namespace NomiWrite.Subscription.Application.Interfaces;

public interface ISubscriptionDbContext
{
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<UserSubscription> UserSubscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
