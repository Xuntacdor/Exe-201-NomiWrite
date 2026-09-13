using Microsoft.EntityFrameworkCore;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Domain.Entities;

namespace NomiWrite.AICoordinator.Application.UnitTests.Persistence;

public sealed class TestGradingDbContext : DbContext, IGradingDbContext
{
    public TestGradingDbContext(DbContextOptions<TestGradingDbContext> options)
        : base(options)
    {
    }

    public DbSet<GradingResult> GradingResults => Set<GradingResult>();
    public DbSet<TutorReviewRequest> TutorReviewRequests => Set<TutorReviewRequest>();
    public DbSet<GradingFeedbackFlag> GradingFeedbackFlags => Set<GradingFeedbackFlag>();
    public DbSet<AiGradingConfig> AiGradingConfigs => Set<AiGradingConfig>();

    public static TestGradingDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestGradingDbContext>()
            .UseInMemoryDatabase($"test-grading-{Guid.NewGuid():N}")
            .Options;
        return new TestGradingDbContext(options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NomiWrite.AICoordinator.Infrastructure.Persistence.GradingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}