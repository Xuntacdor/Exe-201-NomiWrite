using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Application.Exceptions;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Application.Services;

public class QuizService : IQuizService
{
    private const int DefaultQuestionCount = 5;
    private const int MaxQuestionsPerQuiz = 10;
    private const int RecentHistoryWindow = 5;

    private static readonly HashSet<string> AllowedQuestionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "multiple_choice",
        "fill_blank",
        "rewrite"
    };

    private readonly ILearningDbContext _dbContext;
    private readonly IAiQuizProvider _aiQuizProvider;
    private readonly IFallbackQuizProvider _fallbackQuizProvider;
    private readonly IValidator<SubmitQuizAttemptRequestDto> _attemptValidator;
    private readonly ILogger<QuizService> _logger;

    public QuizService(
        ILearningDbContext dbContext,
        IAiQuizProvider aiQuizProvider,
        IFallbackQuizProvider fallbackQuizProvider,
        IValidator<SubmitQuizAttemptRequestDto> attemptValidator,
        ILogger<QuizService> logger)
    {
        _dbContext = dbContext;
        _aiQuizProvider = aiQuizProvider;
        _fallbackQuizProvider = fallbackQuizProvider;
        _attemptValidator = attemptValidator;
        _logger = logger;
    }

    public async Task<QuizDto> GenerateQuizAsync(Guid userId, GenerateQuizRequestDto request)
    {
        var submissionId = request.SubmissionId ?? request.SourceSubmissionId;

        var (grammarErrors, vocab) = await CollectQuizSourcesAsync(userId, submissionId, request);

        if (grammarErrors.Count == 0 && vocab.Count == 0)
        {
            throw new QuizGenerationSourceNotFoundException(
                submissionId?.ToString() ?? "General");
        }

        var generationRequest = new QuizGenerationRequest
        {
            GrammarErrors = grammarErrors,
            Vocabulary = vocab,
            Hint = request.Categories is { Count: > 0 }
                ? $"Focus on the following categories: {string.Join(", ", request.Categories)}."
                : null
        };

        var questions = await GenerateQuestionsAsync(generationRequest);

        if (questions.Count == 0)
        {
            throw new QuizGenerationSourceNotFoundException(
                "Unable to generate quiz questions from the available weak points.");
        }

        var quiz = new Quiz
        {
            UserId = userId,
            SourceSubmissionId = submissionId,
            Category = BuildQuizCategory(request, grammarErrors, vocab),
            Questions = questions,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Quizzes.Add(quiz);
        await _dbContext.SaveChangesAsync();

        return new QuizDto
        {
            Id = quiz.Id,
            UserId = quiz.UserId,
            SourceSubmissionId = quiz.SourceSubmissionId,
            Category = quiz.Category,
            Questions = quiz.Questions.Select(ToSafeQuestion).ToList(),
            CreatedAt = quiz.CreatedAt
        };
    }

    public async Task<QuizDetailDto> GetQuizAsync(Guid userId, Guid quizId)
    {
        var quiz = await _dbContext.Quizzes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz is null)
            throw new QuizNotFoundException(quizId);

        if (quiz.UserId != userId)
            throw new ForbiddenLearningAccessException();

        var isCompleted = await _dbContext.QuizAttempts
            .AsNoTracking()
            .AnyAsync(a => a.QuizId == quizId && a.UserId == userId);

        var detail = new QuizDetailDto
        {
            Id = quiz.Id,
            UserId = quiz.UserId,
            SourceSubmissionId = quiz.SourceSubmissionId,
            Category = quiz.Category,
            Questions = quiz.Questions.Select(ToSafeQuestion).ToList(),
            IsCompleted = isCompleted,
            CreatedAt = quiz.CreatedAt
        };

        // Answers are never exposed before the caller has actually attempted the
        // quiz; this also prevents leaking answer keys through the Network tab.
        if (isCompleted)
        {
            detail.CorrectAnswers = quiz.Questions.ToDictionary(q => q.Id, q => q.CorrectAnswer);
            detail.Explanations = quiz.Questions.ToDictionary(q => q.Id, q => q.Explanation);
        }

        return detail;
    }

    public async Task<QuizAttemptResultDto> SubmitAttemptAsync(Guid userId, SubmitQuizAttemptRequestDto request)
    {
        var validationResult = await _attemptValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var quiz = await _dbContext.Quizzes
            .FirstOrDefaultAsync(q => q.Id == request.QuizId);

        if (quiz is null)
            throw new QuizNotFoundException(request.QuizId);

        if (quiz.UserId != userId)
            throw new ForbiddenLearningAccessException();

        var now = DateTime.UtcNow;
        var score = 0;
        var breakdown = new List<QuizQuestionResultDto>();
        var normalizedAnswers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var question in quiz.Questions)
        {
            var userAnswer = request.Answers.TryGetValue(question.Id, out var answer) ? answer.Trim() : string.Empty;
            var correctAnswer = question.CorrectAnswer.Trim();

            var isCorrect = !string.IsNullOrWhiteSpace(userAnswer) &&
                            string.Equals(userAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
                score++;

            normalizedAnswers[question.Id] = userAnswer;

            breakdown.Add(new QuizQuestionResultDto
            {
                QuestionId = question.Id,
                Question = question.Question,
                UserAnswer = userAnswer,
                CorrectAnswer = correctAnswer,
                IsCorrect = isCorrect,
                Explanation = question.Explanation
            });
        }

        var attempt = new QuizAttempt
        {
            QuizId = quiz.Id,
            UserId = userId,
            Answers = normalizedAnswers,
            Score = score,
            TotalQuestions = quiz.Questions.Count,
            AttemptedAt = now,
            CreatedAt = now
        };

        _dbContext.QuizAttempts.Add(attempt);
        await _dbContext.SaveChangesAsync();

        return new QuizAttemptResultDto
        {
            Id = attempt.Id,
            QuizId = attempt.QuizId,
            UserId = attempt.UserId,
            Answers = attempt.Answers,
            Score = attempt.Score,
            TotalQuestions = attempt.TotalQuestions,
            AttemptedAt = attempt.AttemptedAt,
            QuestionBreakdown = breakdown
        };
    }

    private async Task<(List<GrammarErrorDto>, List<VocabDto>)> CollectQuizSourcesAsync(
        Guid userId,
        Guid? submissionId,
        GenerateQuizRequestDto request)
    {
        if (submissionId.HasValue)
        {
            var grammarErrors = await _dbContext.GrammarErrors
                .AsNoTracking()
                .Where(g => g.UserId == userId && g.SubmissionId == submissionId.Value)
                .OrderByDescending(g => g.CreatedAt)
                .Take(50)
                .Select(GrammarErrorDtoProjection)
                .ToListAsync();

            var vocab = await _dbContext.VocabSuggestions
                .AsNoTracking()
                .Where(v => v.UserId == userId && v.SubmissionId == submissionId.Value)
                .OrderByDescending(v => v.CreatedAt)
                .Take(50)
                .Select(VocabDtoProjection)
                .ToListAsync();

            return (grammarErrors, vocab);
        }

        if (request.VocabularyIds is { Count: > 0 })
        {
            var ids = request.VocabularyIds.Distinct().ToList();
            var vocab = await _dbContext.VocabSuggestions
                .AsNoTracking()
                .Where(v => v.UserId == userId && ids.Contains(v.Id))
                .OrderByDescending(v => v.CreatedAt)
                .Take(50)
                .Select(VocabDtoProjection)
                .ToListAsync();

            var grammarErrors = await _dbContext.GrammarErrors
                .AsNoTracking()
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.CreatedAt)
                .Take(50)
                .Select(GrammarErrorDtoProjection)
                .ToListAsync();

            return (grammarErrors, vocab);
        }

        // Recent error history aggregation: pull the latest weak points across
        // the user's most recent graded submissions.
        var recentSubmissions = await _dbContext.GrammarErrors
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => g.SubmissionId)
            .Distinct()
            .Take(RecentHistoryWindow)
            .ToListAsync();

        var recentGrammar = await _dbContext.GrammarErrors
            .AsNoTracking()
            .Where(g => g.UserId == userId && recentSubmissions.Contains(g.SubmissionId))
            .OrderByDescending(g => g.CreatedAt)
            .Take(50)
            .Select(GrammarErrorDtoProjection)
            .ToListAsync();

        var recentVocab = await _dbContext.VocabSuggestions
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .Take(50)
            .Select(VocabDtoProjection)
            .ToListAsync();

        return (
            request.Categories is { Count: > 0 }
                ? recentGrammar
                    .Where(g => request.Categories.Contains(g.GrammarCategory, StringComparer.OrdinalIgnoreCase))
                    .ToList()
                : recentGrammar,
            recentVocab);
    }

    private async Task<List<QuizQuestionItem>> GenerateQuestionsAsync(QuizGenerationRequest generationRequest)
    {
        var targetCount = DefaultQuestionCount;

        try
        {
            var aiQuestions = await _aiQuizProvider.GenerateQuestionsAsync(generationRequest, targetCount);
            return NormalizeQuestions(aiQuestions);
        }
        catch (Exception exception) when (exception is HttpRequestException
            or InvalidOperationException
            or TaskCanceledException
            or JsonException)
        {
            _logger.LogWarning(exception, "AI quiz generation failed; using deterministic fallback.");
        }

        return NormalizeQuestions(_fallbackQuizProvider.GenerateQuestions(generationRequest, targetCount));
    }

    private static List<QuizQuestionItem> NormalizeQuestions(IEnumerable<QuizQuestionItem> source)
    {
        return source
            .Where(q => !string.IsNullOrWhiteSpace(q.Question))
            .Take(MaxQuestionsPerQuiz)
            .Select(q => new QuizQuestionItem
            {
                Id = string.IsNullOrWhiteSpace(q.Id) ? Guid.NewGuid().ToString("N") : q.Id,
                Category = string.IsNullOrWhiteSpace(q.Category) ? "General" : q.Category,
                Type = NormalizeType(q.Type),
                Question = q.Question,
                Sentence = q.Sentence ?? string.Empty,
                Options = q.Options ?? new List<string>(),
                CorrectAnswer = q.CorrectAnswer ?? string.Empty,
                Explanation = q.Explanation ?? string.Empty
            })
            .ToList();
    }

    private static string NormalizeType(string type)
    {
        if (AllowedQuestionTypes.Contains(type))
            return type.ToLowerInvariant();

        return "multiple_choice";
    }

    private static string BuildQuizCategory(
        GenerateQuizRequestDto request,
        IReadOnlyCollection<GrammarErrorDto> grammarErrors,
        IReadOnlyCollection<VocabDto> vocab)
    {
        if (request.Categories is { Count: > 0 })
            return string.Join(", ", request.Categories.Distinct());

        var parts = grammarErrors
            .Select(g => g.GrammarCategory)
            .Where(c => !string.IsNullOrWhiteSpace(c) && !string.Equals(c, "Khác", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToList();

        if (vocab.Count > 0)
            parts.Add("Vocabulary");

        if (parts.Count == 0)
            parts.Add("Grammar");

        return string.Join(", ", parts.Distinct());
    }

    private static QuizQuestionSafeDto ToSafeQuestion(QuizQuestionItem question)
    {
        return new QuizQuestionSafeDto
        {
            Id = question.Id,
            Category = question.Category,
            Type = question.Type,
            Question = question.Question,
            Sentence = question.Sentence,
            Options = question.Options
        };
    }

    private static System.Linq.Expressions.Expression<Func<GrammarError, GrammarErrorDto>> GrammarErrorDtoProjection =>
        g => new GrammarErrorDto
        {
            Id = g.Id,
            SubmissionId = g.SubmissionId,
            UserId = g.UserId,
            GrammarCategory = g.GrammarCategory,
            Sentence = g.Sentence,
            ErrorPart = g.ErrorPart,
            Suggestion = g.Suggestion,
            Explanation = g.Explanation
        };

    private static System.Linq.Expressions.Expression<Func<VocabSuggestion, VocabDto>> VocabDtoProjection =>
        v => new VocabDto
        {
            Id = v.Id,
            UserId = v.UserId,
            SubmissionId = v.SubmissionId,
            Topic = v.Topic,
            OriginalWord = v.OriginalWord,
            SuggestedWord = v.SuggestedWord,
            ExampleSentence = v.ExampleSentence,
            IsMastered = v.IsMastered,
            CreatedAt = v.CreatedAt
        };
}