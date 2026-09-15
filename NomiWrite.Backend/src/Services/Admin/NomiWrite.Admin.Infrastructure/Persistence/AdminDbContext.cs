using Microsoft.EntityFrameworkCore;
using NomiWrite.Admin.Application.Interfaces;

namespace NomiWrite.Admin.Infrastructure.Persistence;

public class AdminDbContext : DbContext, IAdminDbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

    public DbSet<Domain.Entities.ContentReport> ContentReports => Set<Domain.Entities.ContentReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdminDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Entity is Domain.Common.BaseEntity added)
                added.CreatedAt = now;
            else if (entry.State == EntityState.Modified && entry.Entity is Domain.Common.BaseEntity modified)
                modified.UpdatedAt = now;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
