using Microsoft.EntityFrameworkCore;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Domain.Common;
using NomiWrite.AICoordinator.Domain.Entities;

namespace NomiWrite.AICoordinator.Infrastructure.Persistence;

public class GradingDbContext : DbContext, IGradingDbContext
{
    public GradingDbContext(DbContextOptions<GradingDbContext> options) : base(options) { }

    public DbSet<GradingResult> GradingResults => Set<GradingResult>();
    public DbSet<TutorReviewRequest> TutorReviewRequests => Set<TutorReviewRequest>();
    public DbSet<GradingFeedbackFlag> GradingFeedbackFlags => Set<GradingFeedbackFlag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GradingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInfo()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
    }
}
