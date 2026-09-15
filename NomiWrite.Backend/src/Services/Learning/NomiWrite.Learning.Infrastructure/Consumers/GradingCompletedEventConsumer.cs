using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Domain.Entities;
using NomiWrite.Shared.Contracts.Events.Grading;

namespace NomiWrite.Learning.Infrastructure.Consumers;

public class GradingCompletedEventConsumer : IConsumer<GradingCompletedEvent>
{
    private const string DefaultVocabularyTopic = "Vocabulary";
    private const string DefaultGrammarCategory = "Khác";

    private readonly ILearningDbContext _dbContext;
    private readonly ILogger<GradingCompletedEventConsumer> _logger;

    public GradingCompletedEventConsumer(
        ILearningDbContext dbContext,
        ILogger<GradingCompletedEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GradingCompletedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Consuming GradingCompletedEvent for submission {SubmissionId} (user {UserId}).",
            @event.SubmissionId, @event.UserId);

        var addedCount = 0;

        foreach (var suggestion in @event.VocabularySuggestions ?? new List<GradingVocabularySuggestionEventItem>())
        {
            if (string.IsNullOrWhiteSpace(suggestion.OriginalWord) ||
                suggestion.SuggestedAlternatives == null ||
                suggestion.SuggestedAlternatives.Count == 0)
            {
                continue;
            }

            var suggestedWord = string.Join(" / ", suggestion.SuggestedAlternatives.Where(a => !string.IsNullOrWhiteSpace(a)));

            if (string.IsNullOrWhiteSpace(suggestedWord))
                continue;

            var duplicateExists = await _dbContext.VocabSuggestions
                .AnyAsync(v => v.UserId == @event.UserId &&
                               v.SubmissionId == @event.SubmissionId &&
                               v.OriginalWord == suggestion.OriginalWord &&
                               v.SuggestedWord == suggestedWord);

            if (duplicateExists)
                continue;

            _dbContext.VocabSuggestions.Add(new VocabSuggestion
            {
                UserId = @event.UserId,
                SubmissionId = @event.SubmissionId,
                Topic = DefaultVocabularyTopic,
                OriginalWord = suggestion.OriginalWord,
                SuggestedWord = suggestedWord,
                ExampleSentence = string.IsNullOrWhiteSpace(suggestion.Context)
                    ? $"Given the context, you used '{suggestion.OriginalWord}'. A better option: {suggestedWord}."
                    : suggestion.Context,
                IsMastered = false,
                CreatedAt = @event.CompletedAt
            });

            addedCount++;
        }

        foreach (var error in @event.GrammarErrors ?? new List<GradingGrammarErrorEventItem>())
        {
            if (string.IsNullOrWhiteSpace(error.OriginalText))
                continue;

            var errorPart = error.Explanation ?? error.OriginalText;
            var category = ClassifyGrammarCategory(error);

            var duplicateExists = await _dbContext.GrammarErrors
                .AnyAsync(e => e.UserId == @event.UserId &&
                               e.SubmissionId == @event.SubmissionId &&
                               ((e.ErrorPart ?? string.Empty) == (error.Suggestion ?? string.Empty) ||
                                (e.Sentence ?? string.Empty) == error.OriginalText));

            if (duplicateExists)
                continue;

            _dbContext.GrammarErrors.Add(new GrammarError
            {
                UserId = @event.UserId,
                SubmissionId = @event.SubmissionId,
                GrammarCategory = category,
                Sentence = error.OriginalText,
                ErrorPart = error.Explanation ?? string.Empty,
                Suggestion = error.Suggestion,
                Explanation = error.Explanation ?? string.Empty,
                CreatedAt = @event.CompletedAt
            });

            addedCount++;
        }

        if (addedCount > 0)
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation(
                "Persisted {AddedCount} learning items for submission {SubmissionId}.",
                addedCount, @event.SubmissionId);
        }
    }

    private static string ClassifyGrammarCategory(GradingGrammarErrorEventItem error)
    {
        var text = string.Concat(error.Suggestion, " ", error.Explanation, " ", error.OriginalText).ToLowerInvariant();

        if (text.Contains("hòa hợp", StringComparison.Ordinal) ||
            text.Contains("subject-verb agreement", StringComparison.Ordinal) ||
            text.Contains("agreement", StringComparison.Ordinal))
            return "Hòa hợp chủ ngữ - động từ";

        if (text.Contains("thì", StringComparison.Ordinal) ||
            text.Contains("tense", StringComparison.Ordinal))
            return "Chia thì động từ";

        if (text.Contains("mạo từ", StringComparison.Ordinal) ||
            text.Contains("article", StringComparison.Ordinal))
            return "Mạo từ";

        if (text.Contains("giới từ", StringComparison.Ordinal) ||
            text.Contains("preposition", StringComparison.Ordinal))
            return "Giới từ";

        if (text.Contains("số ít", StringComparison.Ordinal) ||
            text.Contains("số nhiều", StringComparison.Ordinal) ||
            text.Contains("plural", StringComparison.Ordinal) ||
            text.Contains("singular", StringComparison.Ordinal))
            return "Số ít / số nhiều của danh từ";

        if (text.Contains("câu điều kiện", StringComparison.Ordinal) ||
            text.Contains("conditional", StringComparison.Ordinal))
            return "Câu điều kiện";

        if (text.Contains("mệnh đề quan hệ", StringComparison.Ordinal) ||
            text.Contains("relative", StringComparison.Ordinal))
            return "Mệnh đề quan hệ";

        if (text.Contains("bị động", StringComparison.Ordinal) ||
            text.Contains("passive", StringComparison.Ordinal))
            return "Câu bị động";

        if (text.Contains("trật tự từ", StringComparison.Ordinal) ||
            text.Contains("word order", StringComparison.Ordinal))
            return "Trật tự từ trong câu";

        if (text.Contains("liên từ", StringComparison.Ordinal) ||
            text.Contains("conjunction", StringComparison.Ordinal))
            return "Liên từ và từ nối";

        if (text.Contains("đại từ", StringComparison.Ordinal) ||
            text.Contains("pronoun", StringComparison.Ordinal))
            return "Đại từ";

        if (text.Contains("so sánh", StringComparison.Ordinal) ||
            text.Contains("comparative", StringComparison.Ordinal) ||
            text.Contains("superlative", StringComparison.Ordinal))
            return "So sánh hơn / so sánh nhất";

        if (text.Contains("khuyết thiếu", StringComparison.Ordinal) ||
            text.Contains("modal", StringComparison.Ordinal))
            return "Động từ khuyết thiếu";

        if (text.Contains("danh động từ", StringComparison.Ordinal) ||
            text.Contains("gerund", StringComparison.Ordinal) ||
            text.Contains("infinitive", StringComparison.Ordinal))
            return "Danh động từ và động từ nguyên mẫu";

        if (text.Contains("collocation", StringComparison.Ordinal))
            return "Collocation";

        if (text.Contains("dấu câu", StringComparison.Ordinal) ||
            text.Contains("punctuation", StringComparison.Ordinal))
            return "Dấu câu";

        if (text.Contains("dùng từ sai loại", StringComparison.Ordinal) ||
            text.Contains("word form", StringComparison.Ordinal))
            return "Dùng từ sai loại";

        if (text.Contains("cấu trúc câu", StringComparison.Ordinal) ||
            text.Contains("sentence structure", StringComparison.Ordinal))
            return "Cấu trúc câu";

        if (text.Contains("lặp từ", StringComparison.Ordinal) ||
            text.Contains("wordy", StringComparison.Ordinal) ||
            text.Contains("repetition", StringComparison.Ordinal))
            return "Lặp từ / diễn đạt dài dòng";

        return DefaultGrammarCategory;
    }
}