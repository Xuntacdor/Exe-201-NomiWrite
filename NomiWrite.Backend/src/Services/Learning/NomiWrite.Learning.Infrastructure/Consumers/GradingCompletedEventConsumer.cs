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
    private const string ArticleCategory = "M\u1ea1o t\u1eeb";
    private const string SubjectVerbAgreementCategory = "H\u00f2a h\u1ee3p ch\u1ee7 ng\u1eef - \u0111\u1ed9ng t\u1eeb";
    private const string VerbTenseCategory = "Chia th\u00ec \u0111\u1ed9ng t\u1eeb";
    private const string NounNumberCategory = "S\u1ed1 \u00edt / s\u1ed1 nhi\u1ec1u c\u1ee7a danh t\u1eeb";
    private const string PrepositionCategory = "Gi\u1edbi t\u1eeb";
    private const string ConditionalCategory = "C\u00e2u \u0111i\u1ec1u ki\u1ec7n";
    private const string RelativeClauseCategory = "M\u1ec7nh \u0111\u1ec1 quan h\u1ec7";
    private const string PassiveVoiceCategory = "C\u00e2u b\u1ecb \u0111\u1ed9ng";
    private const string WordOrderCategory = "Tr\u1eadt t\u1ef1 t\u1eeb trong c\u00e2u";
    private const string ConnectorCategory = "Li\u00ean t\u1eeb v\u00e0 t\u1eeb n\u1ed1i";
    private const string PronounCategory = "\u0110\u1ea1i t\u1eeb";
    private const string ComparisonCategory = "So s\u00e1nh h\u01a1n / so s\u00e1nh nh\u1ea5t";
    private const string ModalVerbCategory = "\u0110\u1ed9ng t\u1eeb khuy\u1ebft thi\u1ebfu";
    private const string GerundInfinitiveCategory = "Danh \u0111\u1ed9ng t\u1eeb v\u00e0 \u0111\u1ed9ng t\u1eeb nguy\u00ean m\u1eabu";
    private const string SentenceStructureCategory = "C\u1ea5u tr\u00fac c\u00e2u";
    private const string WordFormCategory = "D\u00f9ng t\u1eeb sai lo\u1ea1i";
    private const string CollocationCategory = "Collocation";
    private const string PunctuationCategory = "D\u1ea5u c\u00e2u";
    private const string WordinessCategory = "L\u1eb7p t\u1eeb / di\u1ec5n \u0111\u1ea1t d\u00e0i d\u00f2ng";
    private const string FragmentCategory = "Thi\u1ebfu/th\u1eeba th\u00e0nh ph\u1ea7n c\u00e2u";
    private const string DefaultGrammarCategory = "Kh\u00e1c";

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

        if (ContainsAny(text, "h\u00f2a h\u1ee3p", "subject-verb agreement", "agreement"))
            return SubjectVerbAgreementCategory;

        if (ContainsAny(text, "th\u00ec", "tense"))
            return VerbTenseCategory;

        if (ContainsAny(text, "m\u1ea1o t\u1eeb", "article"))
            return ArticleCategory;

        if (ContainsAny(text, "gi\u1edbi t\u1eeb", "preposition"))
            return PrepositionCategory;

        if (ContainsAny(text, "s\u1ed1 \u00edt", "s\u1ed1 nhi\u1ec1u", "plural", "singular"))
            return NounNumberCategory;

        if (ContainsAny(text, "c\u00e2u \u0111i\u1ec1u ki\u1ec7n", "conditional"))
            return ConditionalCategory;

        if (ContainsAny(text, "m\u1ec7nh \u0111\u1ec1 quan h\u1ec7", "relative"))
            return RelativeClauseCategory;

        if (ContainsAny(text, "b\u1ecb \u0111\u1ed9ng", "passive"))
            return PassiveVoiceCategory;

        if (ContainsAny(text, "tr\u1eadt t\u1ef1 t\u1eeb", "word order"))
            return WordOrderCategory;

        if (ContainsAny(text, "li\u00ean t\u1eeb", "conjunction", "connector"))
            return ConnectorCategory;

        if (ContainsAny(text, "\u0111\u1ea1i t\u1eeb", "pronoun"))
            return PronounCategory;

        if (ContainsAny(text, "so s\u00e1nh", "comparative", "superlative"))
            return ComparisonCategory;

        if (ContainsAny(text, "khuy\u1ebft thi\u1ebfu", "modal"))
            return ModalVerbCategory;

        if (ContainsAny(text, "danh \u0111\u1ed9ng t\u1eeb", "gerund", "infinitive"))
            return GerundInfinitiveCategory;

        if (ContainsAny(text, "collocation"))
            return CollocationCategory;

        if (ContainsAny(text, "d\u1ea5u c\u00e2u", "punctuation"))
            return PunctuationCategory;

        if (ContainsAny(text, "d\u00f9ng t\u1eeb sai lo\u1ea1i", "word form"))
            return WordFormCategory;

        if (ContainsAny(text, "c\u1ea5u tr\u00fac c\u00e2u", "sentence structure"))
            return SentenceStructureCategory;

        if (ContainsAny(text, "l\u1eb7p t\u1eeb", "wordy", "repetition"))
            return WordinessCategory;

        if (ContainsAny(text, "fragment", "run-on", "missing subject", "missing verb"))
            return FragmentCategory;

        return DefaultGrammarCategory;
    }

    private static bool ContainsAny(string text, params string[] needles)
        => needles.Any(needle => text.Contains(needle, StringComparison.Ordinal));
}
