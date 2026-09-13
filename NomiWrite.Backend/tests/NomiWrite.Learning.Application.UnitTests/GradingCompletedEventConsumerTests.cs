using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Learning.Application.UnitTests.Persistence;
using NomiWrite.Learning.Domain.Entities;
using NomiWrite.Learning.Infrastructure.Consumers;
using NomiWrite.Shared.Contracts.Events.Grading;

namespace NomiWrite.Learning.Application.UnitTests;

public class GradingCompletedEventConsumerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid SubmissionId = Guid.NewGuid();

    private static GradingCompletedEventConsumer Build(TestLearningDbContext db)
        => new(db, NullLogger<GradingCompletedEventConsumer>.Instance);

    private static async Task Consume(GradingCompletedEventConsumer consumer, GradingCompletedEvent evt)
    {
        var context = Substitute.For<ConsumeContext<GradingCompletedEvent>>();
        context.Message.Returns(evt);
        await consumer.Consume(context);
    }

    #region U-L11 — vocab dedupe

    [Fact]
    public async Task Consume_DuplicateVocab_NotPersistedAgain()
    {
        var db = TestLearningDbContext.Create();
        db.VocabSuggestions.Add(new VocabSuggestion
        {
            UserId = UserId,
            SubmissionId = SubmissionId,
            Topic = "Vocabulary",
            OriginalWord = "good",
            SuggestedWord = "beneficial / great",
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        var consumer = Build(db);
        await Consume(consumer, TestEvent(vocab: new List<GradingVocabularySuggestionEventItem>
        {
            new("good", new List<string> { "beneficial", "great" }, "context")
        }));

        db.VocabSuggestions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Consume_NewVocab_PersistsJoinedSuggestedWord()
    {
        var db = TestLearningDbContext.Create();

        var consumer = Build(db);
        await Consume(consumer, TestEvent(vocab: new List<GradingVocabularySuggestionEventItem>
        {
            new("happy", new List<string> { "delighted", "content" }, "the happy child")
        }));

        var stored = db.VocabSuggestions.Single();
        stored.UserId.Should().Be(UserId);
        stored.SubmissionId.Should().Be(SubmissionId);
        stored.OriginalWord.Should().Be("happy");
        stored.SuggestedWord.Should().Be("delighted / content");
        stored.Topic.Should().Be("Vocabulary");
        stored.IsMastered.Should().BeFalse();
    }

    [Fact]
    public async Task Consume_VocabWithoutAlternatives_IsSkipped()
    {
        var db = TestLearningDbContext.Create();

        var consumer = Build(db);
        await Consume(consumer, TestEvent(vocab: new List<GradingVocabularySuggestionEventItem>
        {
            new("happy", new List<string> { "  " }, "context")
        }));

        db.VocabSuggestions.Should().BeEmpty();
    }

    #endregion

    #region U-L11 — grammar dedupe + Vietnamese classifier

    [Fact]
    public async Task Consume_DuplicateGrammarBySentence_NotPersistedAgain()
    {
        var db = TestLearningDbContext.Create();
        db.GrammarErrors.Add(new GrammarError
        {
            UserId = UserId,
            SubmissionId = SubmissionId,
            GrammarCategory = "Khác",
            Sentence = "He go to school.",
            ErrorPart = "go",
            Suggestion = "goes",
            Explanation = "Fix verb form.",
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        var consumer = Build(db);
        await Consume(consumer, TestEvent(grammar: new List<GradingGrammarErrorEventItem>
        {
            new("He go to school.", "goes", "Fix the verb form.")
        }));

        db.GrammarErrors.Should().HaveCount(1);
    }

    [Fact]
    public async Task Consume_NewGrammar_ClassifiesVietnameseCategory()
    {
        var db = TestLearningDbContext.Create();

        var consumer = Build(db);
        await Consume(consumer, TestEvent(grammar: new List<GradingGrammarErrorEventItem>
        {
            new("He go to school.", "He goes to school.", "Subject-verb agreement error.")
        }));

        var stored = db.GrammarErrors.Single();
        stored.GrammarCategory.Should().Be("Hòa hợp chủ ngữ - động từ");
        stored.Sentence.Should().Be("He go to school.");
        stored.Suggestion.Should().Be("He goes to school.");
    }

    [Fact]
    public async Task Consume_UnknownCategoryDefaultsToKhac()
    {
        var db = TestLearningDbContext.Create();

        var consumer = Build(db);
        await Consume(consumer, TestEvent(grammar: new List<GradingGrammarErrorEventItem>
        {
            new("Something odd.", "some fix", "random reason text.")
        }));

        db.GrammarErrors.Single().GrammarCategory.Should().Be("Khác");
    }

    [Fact]
    public async Task Consume_TenseText_ClassifiesAsTenseCategory()
    {
        var db = TestLearningDbContext.Create();

        var consumer = Build(db);
        await Consume(consumer, TestEvent(grammar: new List<GradingGrammarErrorEventItem>
        {
            new("She walks yesterday.", "She walked yesterday.", "Wrong verb tense used.")
        }));

        db.GrammarErrors.Single().GrammarCategory.Should().Be("Chia thì động từ");
    }

    #endregion

    private static GradingCompletedEvent TestEvent(
        List<GradingGrammarErrorEventItem>? grammar = null,
        List<GradingVocabularySuggestionEventItem>? vocab = null) => new(
        SubmissionId,
        UserId,
        6.5m,
        DateTime.UtcNow,
        grammar,
        vocab);
}