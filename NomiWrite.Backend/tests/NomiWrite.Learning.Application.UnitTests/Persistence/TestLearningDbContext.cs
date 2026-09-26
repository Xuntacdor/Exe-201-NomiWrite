using Microsoft.EntityFrameworkCore;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Application.UnitTests.Persistence;

public sealed class TestLearningDbContext : DbContext, ILearningDbContext
{
    public TestLearningDbContext(DbContextOptions<TestLearningDbContext> options)
        : base(options)
    {
    }

    public DbSet<VocabSuggestion> VocabSuggestions => Set<VocabSuggestion>();
    public DbSet<GrammarError> GrammarErrors => Set<GrammarError>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<StudyGuide> StudyGuides => Set<StudyGuide>();

    public static TestLearningDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestLearningDbContext>()
            .UseInMemoryDatabase($"test-learning-{Guid.NewGuid():N}")
            .Options;
        return new TestLearningDbContext(options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NomiWrite.Learning.Infrastructure.Persistence.LearningDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}