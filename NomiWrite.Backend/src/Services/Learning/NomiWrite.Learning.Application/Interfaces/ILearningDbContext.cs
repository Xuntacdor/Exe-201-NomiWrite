using Microsoft.EntityFrameworkCore;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Application.Interfaces;

public interface ILearningDbContext
{
    DbSet<VocabSuggestion> VocabSuggestions { get; }
    DbSet<GrammarError> GrammarErrors { get; }
    DbSet<Quiz> Quizzes { get; }
    DbSet<QuizAttempt> QuizAttempts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}