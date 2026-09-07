using Microsoft.EntityFrameworkCore;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Domain.Common;
using NomiWrite.Writing.Domain.Entities;

namespace NomiWrite.Writing.Infrastructure.Persistence;

public class WritingDbContext : DbContext, IWritingDbContext
{
    public WritingDbContext(DbContextOptions<WritingDbContext> options) : base(options) { }

    public DbSet<WritingType> WritingTypes => Set<WritingType>();
    public DbSet<WritingPrompt> WritingPrompts => Set<WritingPrompt>();
    public DbSet<WritingSubmission> WritingSubmissions => Set<WritingSubmission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WritingDbContext).Assembly);
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