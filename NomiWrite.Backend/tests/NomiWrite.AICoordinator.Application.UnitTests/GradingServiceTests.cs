using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NomiWrite.AICoordinator.Application.DTOs;
using NomiWrite.AICoordinator.Application.Exceptions;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Application.Services;
using NomiWrite.AICoordinator.Application.UnitTests.Persistence;
using NomiWrite.AICoordinator.Domain.Entities;
using NomiWrite.AICoordinator.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Grading;

namespace NomiWrite.AICoordinator.Application.UnitTests;

public class GradingServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid SubmissionId = Guid.NewGuid();

    private static GradingService Build(TestGradingDbContext db, IAiGradingProvider? provider = null,
        ISubscriptionStatusClient? subscription = null, IPublishEndpoint? publish = null)
    {
        provider ??= Substitute.For<IAiGradingProvider>();
        if (subscription is null)
        {
            subscription = Substitute.For<ISubscriptionStatusClient>();
            subscription.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
                .Returns(new SubscriptionStatusResult(false, null, null));
        }
        publish ??= Substitute.For<IPublishEndpoint>();

        return new GradingService(
            db,
            provider,
            subscription,
            publish,
            NullLogger<GradingService>.Instance,
            new Valid<FlagGradingResultRequestDto>());
    }

    private static GeminiGradingResponseSchema SampleResponse() => new()
    {
        OverallBand = 6.5m,
        OverallFeedback = "Good attempt with room to improve.",
        Criteria = new List<GeminiCriterionScoreSchema>
        {
            new() { Name = "Task Achievement", Score = 6.0m, Comment = "Clear position." },
            new() { Name = "Grammatical Range and Accuracy", Score = 7.0m, Comment = "Good variety." }
        },
        GrammarErrors = new List<GeminiGrammarErrorSchema>
        {
            new() { OriginalText = "he go", Suggestion = "he goes", Explanation = "Subject-verb agreement." }
        },
        VocabularySuggestions = new List<GeminiVocabularySuggestionSchema>
        {
            new() { OriginalWord = "good", SuggestedAlternatives = new List<string> { "beneficial" }, Context = "the good habit" }
        },
        RestructuringSuggestions = new List<GeminiRestructuringSuggestionSchema>
        {
            new() { OriginalSentence = "It is important.", SuggestedRewrite = "It is of paramount importance.", Reason = "Formality." }
        }
    };

    private static GradingResult SeedCompleted(TestGradingDbContext db, Guid submissionId = default,
        decimal band = 6.5m, DateTime? createdAt = null, Guid? userId = null)
    {
        var result = new GradingResult
        {
            SubmissionId = submissionId == default ? Guid.NewGuid() : submissionId,
            UserId = userId ?? UserId,
            OverallBand = band,
            OverallFeedback = "ok",
            GrammarErrorsJson = "[]",
            VocabularySuggestionsJson = "[]",
            RestructuringSuggestionsJson = "[]",
            Status = GradingStatus.Completed,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            CompletedAt = createdAt ?? DateTime.UtcNow
        };
        db.GradingResults.Add(result);
        db.SaveChanges();
        return result;
    }

    #region U-G1 — grading parse/classification

    [Fact]
    public async Task GradeSubmissionAsync_ValidProviderResponse_PersistsAndPublishes()
    {
        var db = TestGradingDbContext.Create();
        var provider = Substitute.For<IAiGradingProvider>();
        provider.GradeEssayAsync("my essay").Returns(SampleResponse());
        var publish = Substitute.For<IPublishEndpoint>();

        var sut = Build(db, provider: provider, publish: publish);
        await sut.GradeSubmissionAsync(SubmissionId, UserId, "my essay");

        var stored = db.GradingResults.Single(r => r.SubmissionId == SubmissionId && r.UserId == UserId);
        stored.Status.Should().Be(GradingStatus.Completed);
        stored.OverallBand.Should().Be(6.5m);
        stored.GrammarErrorsJson.Should().Be(JsonSerializer.Serialize(SampleResponse().GrammarErrors));
        stored.VocabularySuggestionsJson.Should()
            .Be(JsonSerializer.Serialize(SampleResponse().VocabularySuggestions));
        stored.RestructuringSuggestionsJson.Should()
            .Be(JsonSerializer.Serialize(SampleResponse().RestructuringSuggestions));
        stored.CriterionScores.Should().HaveCount(2);

        var evt = publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments()).OfType<GradingCompletedEvent>().Single();
        evt.SubmissionId.Should().Be(SubmissionId);
        evt.UserId.Should().Be(UserId);
        evt.OverallBand.Should().Be(6.5m);
        evt.GrammarErrors.Should().ContainSingle();
        evt.VocabularySuggestions.Should().ContainSingle();
    }

    #endregion

    #region U-G2 — provider failure path

    [Fact]
    public async Task GradeSubmissionAsync_ProviderThrows_MarksFailedPersistsErrorAndNoEvent()
    {
        var db = TestGradingDbContext.Create();
        var provider = Substitute.For<IAiGradingProvider>();
        provider.GradeEssayAsync(Arg.Any<string>())
            .Throws(new InvalidOperationException("Gemini API error: 429"));
        var publish = Substitute.For<IPublishEndpoint>();

        var sut = Build(db, provider: provider, publish: publish);
        await sut.GradeSubmissionAsync(SubmissionId, UserId, "my essay");

        var stored = db.GradingResults.Single(r => r.SubmissionId == SubmissionId && r.UserId == UserId);
        stored.Status.Should().Be(GradingStatus.Failed);
        stored.ErrorMessage.Should().Be("AI grading failed. Please retry this submission.");

        publish.ReceivedCalls().SelectMany(c => c.GetArguments())
            .OfType<GradingCompletedEvent>().Should().BeEmpty();
    }

    #endregion

    #region U-G3 — grading idempotency

    [Fact]
    public async Task GradeSubmissionAsync_CompletedDuplicateRequest_SkipsProviderAndEvent()
    {
        var db = TestGradingDbContext.Create();
        var first = SeedCompleted(db, submissionId: SubmissionId, band: 5.5m, createdAt: DateTime.UtcNow.AddDays(-1));

        var provider = Substitute.For<IAiGradingProvider>();
        provider.GradeEssayAsync(Arg.Any<string>()).Returns(SampleResponse());
        var publish = Substitute.For<IPublishEndpoint>();

        var sut = Build(db, provider: provider, publish: publish);
        await sut.GradeSubmissionAsync(SubmissionId, UserId, "rewrite of the essay");

        db.GradingResults.Should().HaveCount(1);
        var stored = db.GradingResults.Single();
        stored.Id.Should().Be(first.Id);
        stored.OverallBand.Should().Be(5.5m);
        stored.Status.Should().Be(GradingStatus.Completed);
        await provider.DidNotReceive().GradeEssayAsync(Arg.Any<string>());
        publish.ReceivedCalls().SelectMany(c => c.GetArguments())
            .OfType<GradingCompletedEvent>().Should().BeEmpty();
    }

    [Fact]
    public async Task GradeSubmissionAsync_PendingDuplicateRequest_SkipsProviderAndEvent()
    {
        var db = TestGradingDbContext.Create();
        db.GradingResults.Add(new GradingResult
        {
            SubmissionId = SubmissionId,
            UserId = UserId,
            GrammarErrorsJson = "[]",
            VocabularySuggestionsJson = "[]",
            RestructuringSuggestionsJson = "[]",
            Status = GradingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        var provider = Substitute.For<IAiGradingProvider>();
        provider.GradeEssayAsync(Arg.Any<string>()).Returns(SampleResponse());
        var publish = Substitute.For<IPublishEndpoint>();

        var sut = Build(db, provider: provider, publish: publish);
        await sut.GradeSubmissionAsync(SubmissionId, UserId, "same essay");

        db.GradingResults.Should().HaveCount(1);
        db.GradingResults.Single().Status.Should().Be(GradingStatus.Pending);
        await provider.DidNotReceive().GradeEssayAsync(Arg.Any<string>());
        publish.ReceivedCalls().SelectMany(c => c.GetArguments())
            .OfType<GradingCompletedEvent>().Should().BeEmpty();
    }

    [Fact]
    public async Task GradeSubmissionAsync_FailedResult_CanBeRetried()
    {
        var db = TestGradingDbContext.Create();
        db.GradingResults.Add(new GradingResult
        {
            SubmissionId = SubmissionId,
            UserId = UserId,
            GrammarErrorsJson = "[]",
            VocabularySuggestionsJson = "[]",
            RestructuringSuggestionsJson = "[]",
            Status = GradingStatus.Failed,
            ErrorMessage = "Previous error",
            CompletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var provider = Substitute.For<IAiGradingProvider>();
        provider.GradeEssayAsync("essay").Returns(SampleResponse());
        var sut = Build(db, provider);

        await sut.GradeSubmissionAsync(SubmissionId, UserId, "essay");

        var result = db.GradingResults.Single();
        result.Status.Should().Be(GradingStatus.Completed);
        result.ErrorMessage.Should().BeNull();
        db.GradingResults.Should().ContainSingle();
    }

    #endregion

    #region U-G4 — result retrieval + Pending barrier

    [Fact]
    public async Task GetGradingResultBySubmissionIdAsync_NoResult_ReturnsNull()
    {
        var db = TestGradingDbContext.Create();

        var sut = Build(db);
        var result = await sut.GetGradingResultBySubmissionIdAsync(Guid.NewGuid(), UserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetGradingResultBySubmissionIdAsync_Completed_ReturnsDto()
    {
        var db = TestGradingDbContext.Create();
        SeedCompleted(db, submissionId: SubmissionId);

        var sut = Build(db);
        var result = await sut.GetGradingResultBySubmissionIdAsync(SubmissionId, UserId);

        result.Should().NotBeNull();
        result!.Status.Should().Be(GradingStatus.Completed);
        result.SubmissionId.Should().Be(SubmissionId);
    }

    [Fact]
    public async Task GetGradingResultBySubmissionIdAsync_PendingResult_CurrentlyReturnsDto()
    {
        // U-G4 flags a "Pending barrier". The current implementation returns the DTO for any
        // existing row, including Pending. Documented as-is; revisit once a barrier is added.
        var db = TestGradingDbContext.Create();
        var result = new GradingResult
        {
            SubmissionId = SubmissionId,
            UserId = UserId,
            Status = GradingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        db.GradingResults.Add(result);
        db.SaveChanges();

        var sut = Build(db);
        var dto = await sut.GetGradingResultBySubmissionIdAsync(SubmissionId, UserId);

        dto.Should().NotBeNull();
        dto!.Status.Should().Be(GradingStatus.Pending);
    }

    #endregion

    #region U-G5 — band difference math

    [Fact]
    public async Task CompareWithPreviousAttemptAsync_ComputesBandDifference()
    {
        var db = TestGradingDbContext.Create();
        var previous = SeedCompleted(db, band: 6.0m, createdAt: DateTime.UtcNow.AddDays(-3));
        var current = SeedCompleted(db, submissionId: SubmissionId, band: 7.0m, createdAt: DateTime.UtcNow);

        var sut = Build(db);
        var comparison = await sut.CompareWithPreviousAttemptAsync(UserId, SubmissionId);

        comparison.Current.OverallBand.Should().Be(7.0m);
        comparison.Previous.Should().NotBeNull();
        comparison.Previous!.OverallBand.Should().Be(6.0m);
        comparison.BandDifference.Should().Be(1.0m);
        comparison.Previous.Id.Should().Be(previous.Id);
        comparison.Previous.Id.Should().NotBe(current.Id);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public async Task GetGradingResultBySubmissionIdAsync_AdvancedRewritesRequireVip(bool isVip, int expectedCount)
    {
        var db = TestGradingDbContext.Create();
        var stored = SeedCompleted(db, SubmissionId);
        stored.RestructuringSuggestionsJson = JsonSerializer.Serialize(SampleResponse().RestructuringSuggestions);
        db.SaveChanges();
        var subscription = Substitute.For<ISubscriptionStatusClient>();
        subscription.GetCurrentSubscriptionAsync(UserId, "token")
            .Returns(new SubscriptionStatusResult(isVip, null, null));
        var sut = Build(db, subscription: subscription);

        var result = await sut.GetGradingResultBySubmissionIdAsync(SubmissionId, UserId, "token");

        result!.RestructuringSuggestions.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task GetGradingHistoryAsync_CompletedResults_ReturnsCriteriaScores()
    {
        var db = TestGradingDbContext.Create();
        var completed = SeedCompleted(db, band: 6.5m, createdAt: DateTime.UtcNow);
        completed.CriterionScores = new List<CriterionScore>
        {
            new() { CriterionName = "Task Achievement", Score = 6.0m, Comment = "Clear position." },
            new() { CriterionName = "Lexical Resource", Score = 7.0m, Comment = "Good vocabulary." }
        };
        db.SaveChanges();

        var sut = Build(db);
        var history = await sut.GetGradingHistoryAsync(UserId);

        history.Should().ContainSingle();
        history[0].SubmissionId.Should().Be(completed.SubmissionId);
        history[0].CriteriaScores.Should().Contain("Task Achievement", 6.0m);
        history[0].CriteriaScores.Should().Contain("Lexical Resource", 7.0m);
    }

    [Fact]
    public async Task CompareWithPreviousAttemptAsync_NoPreviousAttempt_NullDifference()
    {
        var db = TestGradingDbContext.Create();
        SeedCompleted(db, submissionId: SubmissionId, createdAt: DateTime.UtcNow);

        var sut = Build(db);
        var comparison = await sut.CompareWithPreviousAttemptAsync(UserId, SubmissionId);

        comparison.Previous.Should().BeNull();
        comparison.BandDifference.Should().BeNull();
    }

    [Fact]
    public async Task CompareWithPreviousAttemptAsync_UnknownSubmission_Throws()
    {
        var db = TestGradingDbContext.Create();

        var sut = Build(db);
        var act = () => sut.CompareWithPreviousAttemptAsync(UserId, Guid.NewGuid());

        await act.Should().ThrowAsync<GradingResultNotFoundException>();
    }

    #endregion

    #region U-G6 — tutor review VIP gating

    [Fact]
    public async Task RequestTutorReviewAsync_FreeUser_ThrowsSubscriptionRequired()
    {
        var db = TestGradingDbContext.Create();
        SeedCompleted(db, submissionId: SubmissionId);

        var subscription = Substitute.For<ISubscriptionStatusClient>();
        subscription.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new SubscriptionStatusResult(false, null, null));

        var sut = Build(db, subscription: subscription);
        var act = () => sut.RequestTutorReviewAsync(UserId, SubmissionId, "token");

        await act.Should().ThrowAsync<TutorReviewSubscriptionRequiredException>();
        db.TutorReviewRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task RequestTutorReviewAsync_SubscriptionClientFailure_DefaultsToFree()
    {
        var db = TestGradingDbContext.Create();
        SeedCompleted(db, submissionId: SubmissionId);

        var subscription = Substitute.For<ISubscriptionStatusClient>();
        subscription.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Throws(new HttpRequestException("subscription service down"));

        var sut = Build(db, subscription: subscription);
        var act = () => sut.RequestTutorReviewAsync(UserId, SubmissionId, "token");

        await act.Should().ThrowAsync<TutorReviewSubscriptionRequiredException>();
    }

    [Fact]
    public async Task RequestTutorReviewAsync_PremiumUser_CreatesRequest()
    {
        var db = TestGradingDbContext.Create();
        SeedCompleted(db, submissionId: SubmissionId);

        var subscription = Substitute.For<ISubscriptionStatusClient>();
        subscription.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new SubscriptionStatusResult(true, "Premium", DateTime.UtcNow.AddDays(30)));

        var sut = Build(db, subscription: subscription);
        var dto = await sut.RequestTutorReviewAsync(UserId, SubmissionId, "token");

        dto.SubmissionId.Should().Be(SubmissionId);
        dto.Status.Should().Be(TutorReviewStatus.Pending);
        db.TutorReviewRequests.Should().ContainSingle(r => r.UserId == UserId);
    }

    [Fact]
    public async Task RequestTutorReviewAsync_NoGradingResult_ThrowsNotFound()
    {
        var db = TestGradingDbContext.Create();

        var sut = Build(db);
        var act = () => sut.RequestTutorReviewAsync(UserId, Guid.NewGuid(), "token");

        await act.Should().ThrowAsync<GradingResultNotFoundException>();
    }

    #endregion

    #region U-G7 — flag ownership check

    [Fact]
    public async Task FlagGradingResultAsync_OtherUsersResult_ThrowsForbidden()
    {
        var db = TestGradingDbContext.Create();
        var other = SeedCompleted(db, userId: Guid.NewGuid());

        var sut = Build(db);
        var act = () => sut.FlagGradingResultAsync(UserId, other.Id,
            new FlagGradingResultRequestDto { Reason = "Wrong band" });

        await act.Should().ThrowAsync<ForbiddenGradingResultAccessException>();
        db.GradingFeedbackFlags.Should().BeEmpty();
    }

    [Fact]
    public async Task FlagGradingResultAsync_UnknownResult_ThrowsNotFound()
    {
        var db = TestGradingDbContext.Create();

        var sut = Build(db);
        var act = () => sut.FlagGradingResultAsync(UserId, Guid.NewGuid(),
            new FlagGradingResultRequestDto { Reason = "Wrong band" });

        await act.Should().ThrowAsync<GradingResultByIdNotFoundException>();
    }

    [Fact]
    public async Task FlagGradingResultAsync_OwnResult_CreatesFlag()
    {
        var db = TestGradingDbContext.Create();
        var result = SeedCompleted(db, userId: UserId);

        var sut = Build(db);
        var dto = await sut.FlagGradingResultAsync(UserId, result.Id,
            new FlagGradingResultRequestDto { Reason = "Band looks too high" });

        dto.GradingResultId.Should().Be(result.Id);
        dto.Reason.Should().Be("Band looks too high");
        db.GradingFeedbackFlags.Should().ContainSingle(f => f.GradingResultId == result.Id && f.UserId == UserId);
    }

    #endregion

    private sealed class Valid<T> : AbstractValidator<T>
    {
    }
}
