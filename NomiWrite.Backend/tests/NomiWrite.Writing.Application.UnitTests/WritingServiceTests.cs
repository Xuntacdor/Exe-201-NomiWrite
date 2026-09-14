using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NomiWrite.Shared.Contracts.Events.Writing;
using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Application.Exceptions;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Application.Services;
using NomiWrite.Writing.Application.UnitTests.Persistence;
using NomiWrite.Writing.Application.Validation;
using NomiWrite.Writing.Domain.Entities;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Application.UnitTests;

public class WritingServiceTests
{
    private static readonly Guid UserA = Guid.NewGuid();
    private static readonly Guid UserB = Guid.NewGuid();

    private static WritingService Build(TestWritingDbContext db,
        IPublishEndpoint? publish = null,
        ISubscriptionStatusClient? subClient = null)
    {
        publish ??= Substitute.For<IPublishEndpoint>();
        subClient ??= DefaultInactiveSub();
        return new WritingService(
            db,
            new CreateSubmissionRequestValidator(),
            new UpdateSubmissionRequestValidator(),
            subClient!,
            publish,
            NullLogger<WritingService>.Instance);
    }

    private static ISubscriptionStatusClient DefaultInactiveSub()
    {
        var sub = Substitute.For<ISubscriptionStatusClient>();
        sub.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new SubscriptionStatusResult(false, null, null));
        return sub;
    }

    private static WritingType SeedType(TestWritingDbContext db, string name = "IELTS Writing")
    {
        var type = new WritingType
        {
            Name = name,
            Category = WritingTypeCategory.ExamFormat,
            Description = "exam",
            CreatedAt = DateTime.UtcNow
        };
        db.WritingTypes.Add(type);
        db.SaveChanges();
        return type;
    }

    private static WritingPrompt SeedPrompt(TestWritingDbContext db, WritingType type,
        bool isVipOnly = false, int? minWords = null, int? maxWords = null, int? timeLimit = null,
        DifficultyLevel difficulty = DifficultyLevel.Intermediate, string? imageUrl = null)
    {
        var prompt = new WritingPrompt
        {
            WritingTypeId = type.Id,
            WritingType = type,
            Title = $"Prompt {Guid.NewGuid():N}",
            Instructions = "Write an essay.",
            Difficulty = difficulty,
            IsActive = true,
            IsVipOnly = isVipOnly,
            MinWords = minWords,
            MaxWords = maxWords,
            TimeLimitMinutes = timeLimit,
            ImageUrl = imageUrl,
            SampleAnswer = "Sample answer text.",
            CreatedAt = DateTime.UtcNow
        };
        db.WritingPrompts.Add(prompt);
        db.SaveChanges();
        return prompt;
    }

    private static async Task<WritingService> BuildWithDraftAsync(TestWritingDbContext db, Guid userId)
    {
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type);
        var sut = Build(db);
        await sut.CreateSubmissionAsync(userId, new CreateSubmissionRequestDto
        {
            WritingPromptId = prompt.Id,
            IsTimed = false
        });
        return sut;
    }

    #region U-W1 — Submission lifecycle (Draft → Submitted)

    [Fact]
    public async Task CreateSubmissionAsync_CreatesDraftWithEmptyContent()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type);

        var sut = Build(db);
        var result = await sut.CreateSubmissionAsync(UserA, new CreateSubmissionRequestDto
        {
            WritingPromptId = prompt.Id,
            IsTimed = false
        });

        result.Status.Should().Be(SubmissionStatus.Draft);
        result.Content.Should().BeEmpty();
        result.WordCount.Should().Be(0);
        db.WritingSubmissions.Should().ContainSingle(s => s.UserId == UserA);
    }

    [Fact]
    public async Task CreateSubmissionAsync_TimedWithLimit_SetsDeadline()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type, timeLimit: 25);

        var sut = Build(db);
        var before = DateTime.UtcNow;
        var result = await sut.CreateSubmissionAsync(UserA, new CreateSubmissionRequestDto
        {
            WritingPromptId = prompt.Id,
            IsTimed = true
        });
        var after = DateTime.UtcNow;

        result.Status.Should().Be(SubmissionStatus.Draft);
        var stored = db.WritingSubmissions.Single();
        stored.IsTimed.Should().BeTrue();
        stored.DeadlineAt.Should().NotBeNull();
        result.IsTimed.Should().BeTrue();
        result.DeadlineAt.Should().NotBeNull();
        stored.DeadlineAt.Should().BeOnOrAfter(before.AddMinutes(25));
        stored.DeadlineAt.Should().BeOnOrBefore(after.AddMinutes(25));
    }

    [Fact]
    public async Task UpdateSubmissionAsync_Draft_UpdatesContentAndWordCount()
    {
        var db = TestWritingDbContext.Create();
        var sut = await BuildWithDraftAsync(db, UserA);

        var result = await sut.UpdateSubmissionAsync(UserA,
            db.WritingSubmissions.Single().Id,
            new UpdateSubmissionRequestDto { Content = "the quick brown fox" });

        result.Content.Should().Be("the quick brown fox");
        result.WordCount.Should().Be(4);
        result.Status.Should().Be(SubmissionStatus.Draft);
    }

    [Fact]
    public async Task SubmitSubmissionAsync_DraftWithContent_SubmitsAndPublishes()
    {
        var db = TestWritingDbContext.Create();
        var sut = await BuildWithDraftAsync(db, UserA);
        var id = db.WritingSubmissions.Single().Id;
        await sut.UpdateSubmissionAsync(UserA, id, new UpdateSubmissionRequestDto { Content = "four small words here" });

        var result = await sut.SubmitSubmissionAsync(UserA, id);

        result.Status.Should().Be(SubmissionStatus.Submitted);
        result.SubmittedAt.Should().NotBeNull();
        db.WritingSubmissions.Single().Status.Should().Be(SubmissionStatus.Submitted);
        db.WritingSubmissions.Single().WritingPromptId.Should().Be(db.WritingPrompts.Single().Id);
    }

    [Fact]
    public async Task GetSubmissionByIdAsync_Draft_ReturnsCurrentContentForResume()
    {
        var db = TestWritingDbContext.Create();
        var sut = await BuildWithDraftAsync(db, UserA);
        var id = db.WritingSubmissions.Single().Id;
        await sut.UpdateSubmissionAsync(UserA, id, new UpdateSubmissionRequestDto { Content = "resume this draft later" });

        var result = await sut.GetSubmissionByIdAsync(UserA, id);

        result.Status.Should().Be(SubmissionStatus.Draft);
        result.Content.Should().Be("resume this draft later");
        result.WordCount.Should().Be(4);
    }

    [Fact]
    public async Task CreateSubmissionAsync_SamePromptAfterSubmit_AllowsRewriteAttempt()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type);
        var sut = Build(db);

        var first = await sut.CreateSubmissionAsync(UserA, new CreateSubmissionRequestDto { WritingPromptId = prompt.Id });
        await sut.UpdateSubmissionAsync(UserA, first.Id, new UpdateSubmissionRequestDto { Content = "first attempt content" });
        await sut.SubmitSubmissionAsync(UserA, first.Id);
        var second = await sut.CreateSubmissionAsync(UserA, new CreateSubmissionRequestDto { WritingPromptId = prompt.Id });

        second.Id.Should().NotBe(first.Id);
        second.WritingPromptId.Should().Be(prompt.Id);
        second.Status.Should().Be(SubmissionStatus.Draft);
        db.WritingSubmissions.Where(s => s.UserId == UserA && s.WritingPromptId == prompt.Id).Should().HaveCount(2);
    }

    [Fact]
    public async Task SubmitSubmissionAsync_AlreadySubmitted_Throws()
    {
        var db = TestWritingDbContext.Create();
        var sut = await BuildWithDraftAsync(db, UserA);
        var id = db.WritingSubmissions.Single().Id;
        await sut.UpdateSubmissionAsync(UserA, id, new UpdateSubmissionRequestDto { Content = "enough content here" });
        await sut.SubmitSubmissionAsync(UserA, id);

        var act = () => sut.SubmitSubmissionAsync(UserA, id);

        await act.Should().ThrowAsync<SubmissionNotEditableException>();
    }

    [Fact]
    public async Task SubmitSubmissionAsync_EmptyContent_Throws()
    {
        var db = TestWritingDbContext.Create();
        var sut = await BuildWithDraftAsync(db, UserA);

        var act = () => sut.SubmitSubmissionAsync(UserA, db.WritingSubmissions.Single().Id);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task SubmitSubmissionAsync_BelowMinWords_Throws()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type, minWords: 10);
        var sut = Build(db);
        await sut.CreateSubmissionAsync(UserA, new CreateSubmissionRequestDto { WritingPromptId = prompt.Id });
        var id = db.WritingSubmissions.Single().Id;
        await sut.UpdateSubmissionAsync(UserA, id, new UpdateSubmissionRequestDto { Content = "short" });

        var act = () => sut.SubmitSubmissionAsync(UserA, id);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*at least 10 words*");
    }

    [Fact]
    public async Task SubmitSubmissionAsync_AboveMaxWords_Throws()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type, maxWords: 5);
        var sut = Build(db);
        await sut.CreateSubmissionAsync(UserA, new CreateSubmissionRequestDto { WritingPromptId = prompt.Id });
        var id = db.WritingSubmissions.Single().Id;
        await sut.UpdateSubmissionAsync(UserA, id,
            new UpdateSubmissionRequestDto { Content = "one two three four five six seven" });

        var act = () => sut.SubmitSubmissionAsync(UserA, id);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*cannot exceed 5 words*");
    }

    [Fact]
    public async Task SubmitSubmissionAsync_AfterDeadline_FlagsLate()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type, timeLimit: 20);
        var sut = Build(db);
        await sut.CreateSubmissionAsync(UserA, new CreateSubmissionRequestDto
        {
            WritingPromptId = prompt.Id,
            IsTimed = true
        });
        var id = db.WritingSubmissions.Single().Id;

        var stored = db.WritingSubmissions.Single();
        stored.DeadlineAt = DateTime.UtcNow.AddMinutes(-5);
        db.SaveChanges();

        await sut.UpdateSubmissionAsync(UserA, id, new UpdateSubmissionRequestDto { Content = "late but submitted" });
        var result = await sut.SubmitSubmissionAsync(UserA, id);

        result.SubmittedLate.Should().BeTrue();
    }

    #endregion

    #region U-W2 — Ownership guard

    [Fact]
    public async Task GetSubmissionByIdAsync_OtherUsersSubmission_Throws()
    {
        var db = TestWritingDbContext.Create();
        var sut = await BuildWithDraftAsync(db, UserA);
        var id = db.WritingSubmissions.Single().Id;

        var act = () => sut.GetSubmissionByIdAsync(UserB, id);

        await act.Should().ThrowAsync<ForbiddenSubmissionAccessException>();
    }

    [Fact]
    public async Task UpdateSubmissionAsync_OtherUsersSubmission_Throws()
    {
        var db = TestWritingDbContext.Create();
        var sut = await BuildWithDraftAsync(db, UserA);
        var id = db.WritingSubmissions.Single().Id;

        var act = () => sut.UpdateSubmissionAsync(UserB, id, new UpdateSubmissionRequestDto { Content = "hijack" });

        await act.Should().ThrowAsync<ForbiddenSubmissionAccessException>();
        db.WritingSubmissions.Single().Content.Should().BeEmpty();
    }

    [Fact]
    public async Task SubmitSubmissionAsync_OtherUsersSubmission_Throws()
    {
        var db = TestWritingDbContext.Create();
        var sut = await BuildWithDraftAsync(db, UserA);
        var id = db.WritingSubmissions.Single().Id;

        var act = () => sut.SubmitSubmissionAsync(UserB, id);

        await act.Should().ThrowAsync<ForbiddenSubmissionAccessException>();
    }

    [Fact]
    public async Task GetSubmissionByIdAsync_UnknownSubmission_Throws()
    {
        var db = TestWritingDbContext.Create();

        var sut = Build(db);
        var act = () => sut.GetSubmissionByIdAsync(UserA, Guid.NewGuid());

        await act.Should().ThrowAsync<SubmissionNotFoundException>();
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_OnlyReturnsOwn()
    {
        var db = TestWritingDbContext.Create();
        var sut = await BuildWithDraftAsync(db, UserA);
        var id = db.WritingSubmissions.Single().Id;
        await sut.UpdateSubmissionAsync(UserA, id, new UpdateSubmissionRequestDto { Content = "some content" });

        db.WritingSubmissions.Add(new WritingSubmission
        {
            UserId = UserB,
            WritingPromptId = db.WritingSubmissions.Single().WritingPromptId,
            Status = SubmissionStatus.Draft,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        var items = await sut.GetUserSubmissionsAsync(UserA);
        items.Should().ContainSingle(s => s.Id == id);
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_IncludesTimingAndLateMetadata()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type, timeLimit: 20);
        var sut = Build(db);
        var draft = await sut.CreateSubmissionAsync(UserA, new CreateSubmissionRequestDto
        {
            WritingPromptId = prompt.Id,
            IsTimed = true
        });
        var stored = db.WritingSubmissions.Single();
        stored.DeadlineAt = DateTime.UtcNow.AddMinutes(-1);
        db.SaveChanges();
        await sut.UpdateSubmissionAsync(UserA, draft.Id, new UpdateSubmissionRequestDto { Content = "late timed submission" });
        await sut.SubmitSubmissionAsync(UserA, draft.Id);

        var items = await sut.GetUserSubmissionsAsync(UserA);

        var item = items.Single();
        item.IsTimed.Should().BeTrue();
        item.DeadlineAt.Should().NotBeNull();
        item.SubmittedLate.Should().BeTrue();
    }

    #endregion

    #region U-W3 — WritingSubmittedEvent publication

    [Fact]
    public async Task SubmitSubmissionAsync_PublishesEventWithCorrectPayload()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type);
        var publish = Substitute.For<IPublishEndpoint>();
        var sut = Build(db, publish: publish);

        await sut.CreateSubmissionAsync(UserA, new CreateSubmissionRequestDto { WritingPromptId = prompt.Id });
        var id = db.WritingSubmissions.Single().Id;
        await sut.UpdateSubmissionAsync(UserA, id, new UpdateSubmissionRequestDto { Content = "five tiny words written" });
        await sut.SubmitSubmissionAsync(UserA, id);

        var evt = publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments())
            .OfType<WritingSubmittedEvent>()
            .Single();

        evt.SubmissionId.Should().Be(id);
        evt.UserId.Should().Be(UserA);
        evt.WritingPromptId.Should().Be(prompt.Id);
        evt.Content.Should().Be("five tiny words written");
        evt.WordCount.Should().Be(4);
        evt.SubmittedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task SubmitSubmissionAsync_FailedWordCount_DoesNotPublish()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type, minWords: 100);
        var publish = Substitute.For<IPublishEndpoint>();
        var sut = Build(db, publish: publish);
        await sut.CreateSubmissionAsync(UserA, new CreateSubmissionRequestDto { WritingPromptId = prompt.Id });
        var id = db.WritingSubmissions.Single().Id;
        await sut.UpdateSubmissionAsync(UserA, id, new UpdateSubmissionRequestDto { Content = "too short" });

        var act = () => sut.SubmitSubmissionAsync(UserA, id);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();

        publish.ReceivedCalls().Should().BeEmpty();
    }

    #endregion

    #region U-W4 — VIP gating + fail-soft subscription client

    [Fact]
    public async Task GetSampleAnswerAsync_FreeUser_ThrowsSubscriptionRequired()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type);

        var sut = Build(db);
        var act = () => sut.GetSampleAnswerAsync(UserA, prompt.Id, null);

        await act.Should().ThrowAsync<SubscriptionRequiredException>();
    }

    [Fact]
    public async Task GetSampleAnswerAsync_ActiveSubscription_ReturnsAnswer()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type);
        var sub = Substitute.For<ISubscriptionStatusClient>();
        sub.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new SubscriptionStatusResult(true, "premium", DateTime.UtcNow.AddDays(30)));

        var sut = Build(db, subClient: sub);
        var result = await sut.GetSampleAnswerAsync(UserA, prompt.Id, "token");

        result.SampleAnswer.Should().Be("Sample answer text.");
    }

    [Fact]
    public async Task GetSampleAnswerAsync_SubscriptionClientThrows_FailsSoftToNonVip()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type);
        var sub = Substitute.For<ISubscriptionStatusClient>();
        sub.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Throws(new HttpRequestException("subscription service down"));

        var sut = Build(db, subClient: sub);
        var act = () => sut.GetSampleAnswerAsync(UserA, prompt.Id, null);

        await act.Should().ThrowAsync<SubscriptionRequiredException>();
    }

    [Fact]
    public async Task GetPromptsAsync_FreeUser_DoesNotSeeVipPrompts()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var normal = SeedPrompt(db, type);
        var vip = SeedPrompt(db, type, isVipOnly: true);

        var sut = Build(db);
        var prompts = await sut.GetPromptsAsync(null, null, random: false, userId: UserA);

        prompts.Should().Contain(p => p.Id == normal.Id);
        prompts.Should().NotContain(p => p.Id == vip.Id);
    }

    [Fact]
    public async Task GetPromptsAsync_VipUser_SeesVipPrompts()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var normal = SeedPrompt(db, type);
        var vip = SeedPrompt(db, type, isVipOnly: true);
        var sub = Substitute.For<ISubscriptionStatusClient>();
        sub.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new SubscriptionStatusResult(true, "premium", DateTime.UtcNow.AddDays(30)));

        var sut = Build(db, subClient: sub);
        var prompts = await sut.GetPromptsAsync(null, null, random: false, userId: UserA);

        prompts.Should().Contain(p => p.Id == normal.Id);
        prompts.Should().Contain(p => p.Id == vip.Id);
    }

    [Fact]
    public async Task CreateSubmissionAsync_FreeUserCannotStartVipPromptById()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var vip = SeedPrompt(db, type, isVipOnly: true);

        var sut = Build(db);
        var act = () => sut.CreateSubmissionAsync(UserA, new CreateSubmissionRequestDto { WritingPromptId = vip.Id });

        await act.Should().ThrowAsync<SubscriptionRequiredException>();
    }

    [Fact]
    public async Task CreateSubmissionAsync_VipUserCanStartVipPrompt()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var vip = SeedPrompt(db, type, isVipOnly: true);
        var sub = Substitute.For<ISubscriptionStatusClient>();
        sub.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new SubscriptionStatusResult(true, "premium", DateTime.UtcNow.AddDays(30)));

        var sut = Build(db, subClient: sub);
        var result = await sut.CreateSubmissionAsync(
            UserA,
            new CreateSubmissionRequestDto { WritingPromptId = vip.Id },
            "token");

        result.WritingPromptId.Should().Be(vip.Id);
        result.Status.Should().Be(SubmissionStatus.Draft);
    }

    [Fact]
    public async Task GetPromptsAsync_AnonymousVisitor_NeverSeesVipPrompts()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var vip = SeedPrompt(db, type, isVipOnly: true);

        var sut = Build(db);
        var prompts = await sut.GetPromptsAsync(null, null, random: false, userId: null);

        prompts.Should().NotContain(p => p.Id == vip.Id);
    }

    [Fact]
    public async Task GetPromptsAsync_FiltersByTypeAndDifficulty()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db, "Part 1");
        var beginner = SeedPrompt(db, type);
        var advanced = SeedPrompt(db, type, difficulty: DifficultyLevel.Advanced);

        var sut = Build(db);
        var prompts = await sut.GetPromptsAsync(type.Id, DifficultyLevel.Advanced, random: false, userId: UserA);

        prompts.Should().ContainSingle(p => p.Id == advanced.Id);
    }

    [Fact]
    public async Task GetPromptsAsync_Random_ReturnsSinglePromptFromFilteredSet()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        SeedPrompt(db, type, difficulty: DifficultyLevel.Beginner);
        var advanced = SeedPrompt(db, type, difficulty: DifficultyLevel.Advanced);

        var sut = Build(db);
        var prompts = await sut.GetPromptsAsync(type.Id, DifficultyLevel.Advanced, random: true, userId: UserA);

        prompts.Should().ContainSingle(p => p.Id == advanced.Id);
    }

    [Fact]
    public async Task GetPromptByIdAsync_ImagePrompt_ReturnsImageUrl()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = SeedPrompt(db, type, imageUrl: "https://example.com/chart.png");

        var sut = Build(db);
        var result = await sut.GetPromptByIdAsync(prompt.Id);

        result.ImageUrl.Should().Be("https://example.com/chart.png");
    }

    #endregion
}
