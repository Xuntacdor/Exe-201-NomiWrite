using Microsoft.EntityFrameworkCore;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Domain.Common;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Infrastructure.Persistence;

public class LearningDbContext : DbContext, ILearningDbContext
{
    public LearningDbContext(DbContextOptions<LearningDbContext> options) : base(options) { }

    public DbSet<VocabSuggestion> VocabSuggestions => Set<VocabSuggestion>();
    public DbSet<GrammarError> GrammarErrors => Set<GrammarError>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LearningDbContext).Assembly);
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