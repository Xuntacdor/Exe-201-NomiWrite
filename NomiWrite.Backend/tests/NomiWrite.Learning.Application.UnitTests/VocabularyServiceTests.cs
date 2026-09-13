using FluentAssertions;
using NomiWrite.Learning.Application.Exceptions;
using NomiWrite.Learning.Application.Services;
using NomiWrite.Learning.Application.UnitTests.Persistence;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Application.UnitTests;

public class VocabularyServiceTests
{
    private static readonly Guid UserA = Guid.NewGuid();
    private static readonly Guid UserB = Guid.NewGuid();

    private static VocabSuggestion Seed(TestLearningDbContext db, Guid userId, string word,
        bool mastered = false, string topic = "Education")
    {
        var vocab = new VocabSuggestion
        {
            UserId = userId,
            SubmissionId = null,
            Topic = topic,
            OriginalWord = word,
            SuggestedWord = $"better-{word}",
            ExampleSentence = $"example for {word}",
            IsMastered = mastered,
            CreatedAt = DateTime.UtcNow
        };
        db.VocabSuggestions.Add(vocab);
        db.SaveChanges();
        return vocab;
    }

    #region U-L1 — IDOR on mastery update

    [Fact]
    public async Task UpdateMasteredAsync_AnotherUsersVocab_ThrowsForbidden()
    {
        var db = TestLearningDbContext.Create();
        var bVocab = Seed(db, UserB, "good");

        var sut = new VocabularyService(db);
        var act = () => sut.UpdateMasteredAsync(UserA, bVocab.Id, true);

        await act.Should().ThrowAsync<ForbiddenLearningAccessException>();
        db.VocabSuggestions.Single(v => v.Id == bVocab.Id).IsMastered.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateMasteredAsync_UnknownVocab_ThrowsNotFound()
    {
        var db = TestLearningDbContext.Create();

        var sut = new VocabularyService(db);
        var act = () => sut.UpdateMasteredAsync(UserA, Guid.NewGuid(), true);

        await act.Should().ThrowAsync<VocabSuggestionNotFoundException>();
    }

    [Fact]
    public async Task UpdateMasteredAsync_OwnVocab_UpdatesFlag()
    {
        var db = TestLearningDbContext.Create();
        var vocab = Seed(db, UserA, "good");

        var sut = new VocabularyService(db);
        var result = await sut.UpdateMasteredAsync(UserA, vocab.Id, true);

        result.IsMastered.Should().BeTrue();
        result.Id.Should().Be(vocab.Id);
        db.VocabSuggestions.Single(v => v.Id == vocab.Id).IsMastered.Should().BeTrue();
    }

    #endregion

    #region Listing filters + ownership

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyOwnItems()
    {
        var db = TestLearningDbContext.Create();
        Seed(db, UserA, "good");
        Seed(db, UserB, "bad");

        var sut = new VocabularyService(db);
        var page = await sut.GetAllAsync(UserA, null, null, 1, 20);

        page.Items.Should().ContainSingle(v => v.OriginalWord == "good");
        page.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByTopicAndMastered()
    {
        var db = TestLearningDbContext.Create();
        Seed(db, UserA, "good", mastered: true, topic: "Education");
        Seed(db, UserA, "nice", mastered: false, topic: "Education");
        Seed(db, UserA, "suite", mastered: true, topic: "Business");

        var sut = new VocabularyService(db);
        var page = await sut.GetAllAsync(UserA, "education", true, 1, 20);

        page.Items.Should().ContainSingle(v => v.OriginalWord == "good");
        page.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_TopicFilterIsCaseInsensitive()
    {
        var db = TestLearningDbContext.Create();
        Seed(db, UserA, "good", topic: "Education");

        var sut = new VocabularyService(db);
        var page = await sut.GetAllAsync(UserA, "EDUCATION", null, 1, 20);

        page.TotalCount.Should().Be(1);
    }

    #endregion
}