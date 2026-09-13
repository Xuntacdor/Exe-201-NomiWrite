using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NomiWrite.User.Application.DTOs;
using NomiWrite.User.Application.Exceptions;
using NomiWrite.User.Application.Interfaces;
using NomiWrite.User.Application.Services;
using NomiWrite.User.Application.UnitTests.Persistence;
using NomiWrite.User.Application.Validation;
using NomiWrite.User.Domain.Entities;
using NomiWrite.User.Domain.Enums;
using NomiWrite.User.Infrastructure.Persistence;

namespace NomiWrite.User.Application.UnitTests;

public class UserProfileServiceTests
{
    private static readonly Guid UserA = Guid.NewGuid();
    private static readonly Guid UserB = Guid.NewGuid();

    private static UserProfileService Build(UserDbContext db,
        ISubscriptionStatusClient? subscription = null,
        IWritingHistoryClient? writing = null,
        IGradingHistoryClient? grading = null)
    {
        subscription ??= DefaultSubscriptionClient();
        writing ??= DefaultWritingClient();
        grading ??= DefaultGradingClient();
        return new UserProfileService(
            db,
            new UpdateProfileRequestValidator(),
            subscription!,
            writing!,
            grading!,
            NullLogger<UserProfileService>.Instance);
    }

    private static ISubscriptionStatusClient DefaultSubscriptionClient()
    {
        var sub = Substitute.For<ISubscriptionStatusClient>();
        sub.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new SubscriptionStatusResult(false, null, null));
        return sub;
    }

    private static IWritingHistoryClient DefaultWritingClient()
    {
        var writing = Substitute.For<IWritingHistoryClient>();
        writing.GetSubmissionsAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(Array.Empty<WritingSubmissionSummary>());
        return writing;
    }

    private static IGradingHistoryClient DefaultGradingClient()
    {
        var grading = Substitute.For<IGradingHistoryClient>();
        grading.GetHistoryAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(Array.Empty<GradingHistoryEntry>());
        return grading;
    }

    private static UserProfile SeedProfile(UserDbContext db, Guid userId, string name = "Alice")
    {
        var profile = new UserProfile
        {
            UserId = userId,
            DisplayName = name,
            AvatarUrl = null,
            Bio = null,
            TargetExam = "IELTS",
            TargetBand = 6.5m,
            TargetExamDate = DateTime.UtcNow.AddMonths(3),
            EnglishLevel = EnglishLevel.Intermediate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.UserProfiles.Add(profile);
        db.SaveChanges();
        return profile;
    }

    #region U-U1 — profile creation idempotency

    [Fact]
    public async Task CreateProfile_CreatesProfileWithDisplayName()
    {
        var db = TestUserDbContext.Create();

        var sut = Build(db);
        await sut.CreateProfileFromRegistrationAsync(UserA, "Alice");

        var stored = db.UserProfiles.Single();
        stored.UserId.Should().Be(UserA);
        stored.DisplayName.Should().Be("Alice");
    }

    [Fact]
    public async Task CreateProfile_DuplicateEvent_IsIdempotent()
    {
        var db = TestUserDbContext.Create();
        var sut = Build(db);

        await sut.CreateProfileFromRegistrationAsync(UserA, "Alice");
        await sut.CreateProfileFromRegistrationAsync(UserA, "Alice");

        db.UserProfiles.Should().ContainSingle(p => p.UserId == UserA);
    }

    #endregion

    #region U-U2 — GetProgressAsync aggregation math + fail-soft

    [Fact]
    public async Task GetProgress_BandHistory_OrderedByDate()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);
        var grading = Substitute.For<IGradingHistoryClient>();
        grading.GetHistoryAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new List<GradingHistoryEntry>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), 7.5m, new DateTime(2026, 9, 10, 10, 0, 0, DateTimeKind.Utc), new Dictionary<string, decimal> { ["Grammar"] = 7m }),
                new(Guid.NewGuid(), Guid.NewGuid(), 6.0m, new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc), new Dictionary<string, decimal> { ["Grammar"] = 6m }),
                new(Guid.NewGuid(), Guid.NewGuid(), 5.5m, new DateTime(2026, 8, 20, 10, 0, 0, DateTimeKind.Utc), new Dictionary<string, decimal> { ["Grammar"] = 5m })
            });

        var sut = Build(db, grading: grading);
        var result = await sut.GetProgressAsync(UserA, null);

        result.BandHistory.Select(b => b.Band).Should().Equal(5.5m, 6.0m, 7.5m);
    }

    [Fact]
    public async Task GetProgress_CurrentStreak_CountsConsecutiveDaysToToday()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);
        var today = DateTime.UtcNow.Date;
        var writing = Substitute.For<IWritingHistoryClient>();
        writing.GetSubmissionsAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new List<WritingSubmissionSummary>
            {
                new(Guid.NewGuid(), today, "Submitted"),
                new(Guid.NewGuid(), today.AddDays(-1), "Submitted"),
                new(Guid.NewGuid(), today.AddDays(-2), "Submitted"),
                new(Guid.NewGuid(), today.AddDays(-5), "Submitted")
            });

        var sut = Build(db, writing: writing);
        var result = await sut.GetProgressAsync(UserA, null);

        result.CurrentStreak.Should().Be(3);
        result.TotalSubmissions.Should().Be(4);
    }

    [Fact]
    public async Task GetProgress_StreakWithGap_Breaks()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);
        var today = DateTime.UtcNow.Date;
        var writing = Substitute.For<IWritingHistoryClient>();
        writing.GetSubmissionsAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new List<WritingSubmissionSummary>
            {
                new(Guid.NewGuid(), today, "Submitted"),
                new(Guid.NewGuid(), today.AddDays(-2), "Submitted")
            });

        var sut = Build(db, writing: writing);
        var result = await sut.GetProgressAsync(UserA, null);

        result.CurrentStreak.Should().Be(1);
    }

    [Fact]
    public async Task GetProgress_NoSubmissions_StreakZero()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);

        var sut = Build(db);
        var result = await sut.GetProgressAsync(UserA, null);

        result.CurrentStreak.Should().Be(0);
        result.TotalSubmissions.Should().Be(0);
    }

    [Fact]
    public async Task GetProgress_StrengthsWeaknesses_SingleCriterion()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);
        var grading = Substitute.For<IGradingHistoryClient>();
        grading.GetHistoryAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new List<GradingHistoryEntry>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), 6.5m, DateTime.UtcNow, new Dictionary<string, decimal>
                {
                    ["Grammar"] = 7m
                })
            });

        var sut = Build(db, grading: grading);
        var result = await sut.GetProgressAsync(UserA, null);

        result.StrengthsWeaknesses.Should().Be("Grammar is your strongest area.");
    }

    [Fact]
    public async Task GetProgress_StrengthsWeaknesses_AveragesAcrossRecentResults()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);
        var grading = Substitute.For<IGradingHistoryClient>();
        grading.GetHistoryAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new List<GradingHistoryEntry>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), 6m, DateTime.UtcNow.AddDays(-1), new Dictionary<string, decimal>
                {
                    ["Grammar"] = 8m, ["Task Response"] = 3m
                }),
                new(Guid.NewGuid(), Guid.NewGuid(), 7m, DateTime.UtcNow, new Dictionary<string, decimal>
                {
                    ["Grammar"] = 6m, ["Task Response"] = 6m
                })
            });

        var sut = Build(db, grading: grading);
        var result = await sut.GetProgressAsync(UserA, null);

        result.StrengthsWeaknesses.Should().Contain("Grammar");
        result.StrengthsWeaknesses.Should().Contain("Task Response");
        result.StrengthsWeaknesses!.Should().StartWith("Grammar is your strongest area; Task Response needs");
    }

    [Fact]
    public async Task GetProgress_Badges_ThresholdsAndBandAchievements()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);
        var writing = Substitute.For<IWritingHistoryClient>();
        writing.GetSubmissionsAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(Enumerable.Range(0, 10).Select(i => new WritingSubmissionSummary(
                Guid.NewGuid(), DateTime.UtcNow.AddDays(-i), "Submitted")).ToList());
        var grading = Substitute.For<IGradingHistoryClient>();
        grading.GetHistoryAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new List<GradingHistoryEntry>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), 7.2m, DateTime.UtcNow, new Dictionary<string, decimal>())
            });

        var sut = Build(db, writing: writing, grading: grading);
        var result = await sut.GetProgressAsync(UserA, null);

        result.Badges.Single(b => b.Name == "First Steps").Achieved.Should().BeTrue();
        result.Badges.Single(b => b.Name == "Getting Started").Achieved.Should().BeTrue();
        result.Badges.Single(b => b.Name == "Dedicated Writer").Achieved.Should().BeFalse();
        result.Badges.Single(b => b.Name == "Band 7 Achiever").Achieved.Should().BeTrue();
        result.Badges.Single(b => b.Name == "Band 8 Achiever").Achieved.Should().BeFalse();
    }

    [Fact]
    public async Task GetProgress_WritingHistoryThrows_FailsSoft()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);
        var writing = Substitute.For<IWritingHistoryClient>();
        writing.GetSubmissionsAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Throws(new HttpRequestException("writing service down"));

        var sut = Build(db, writing: writing);
        var result = await sut.GetProgressAsync(UserA, null);

        result.TotalSubmissions.Should().Be(0);
        result.CurrentStreak.Should().Be(0);
    }

    [Fact]
    public async Task GetProgress_GradingHistoryThrows_FailsSoft()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);
        var grading = Substitute.For<IGradingHistoryClient>();
        grading.GetHistoryAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Throws(new HttpRequestException("grading service down"));

        var sut = Build(db, grading: grading);
        var result = await sut.GetProgressAsync(UserA, null);

        result.BandHistory.Should().BeEmpty();
        result.StrengthsWeaknesses.Should().BeNull();
    }

    [Fact]
    public async Task GetProgress_UnknownUser_Throws()
    {
        var db = TestUserDbContext.Create();

        var sut = Build(db);
        var act = () => sut.GetProgressAsync(UserA, null);

        await act.Should().ThrowAsync<ProfileNotFoundException>();
    }

    #endregion

    #region U-U3 — profile update validation

    [Fact]
    public async Task UpdateProfile_TargetBandOutOfRange_Throws()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);

        var sut = Build(db);
        var act = () => sut.UpdateProfileAsync(UserA, new UpdateProfileRequestDto { TargetBand = 9.5m });

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*between 0 and 9*");
    }

    [Fact]
    public async Task UpdateProfile_DisplayNameTooLong_Throws()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);

        var sut = Build(db);
        var act = () => sut.UpdateProfileAsync(UserA,
            new UpdateProfileRequestDto { DisplayName = new string('a', 101) });

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*100*");
    }

    [Fact]
    public async Task UpdateProfile_PartialUpdate_PreservesUnchangedFields()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);

        var sut = Build(db);
        var result = await sut.UpdateProfileAsync(UserA, new UpdateProfileRequestDto { DisplayName = "Bob" });

        result.DisplayName.Should().Be("Bob");
        result.TargetBand.Should().Be(6.5m);
        result.TargetExam.Should().Be("IELTS");
        result.Bio.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProfile_UnknownUser_Throws()
    {
        var db = TestUserDbContext.Create();

        var sut = Build(db);
        var act = () => sut.UpdateProfileAsync(UserB, new UpdateProfileRequestDto { DisplayName = "X" });

        await act.Should().ThrowAsync<ProfileNotFoundException>();
    }

    [Fact]
    public async Task GetProfile_UnknownUser_Throws()
    {
        var db = TestUserDbContext.Create();

        var sut = Build(db);
        var act = () => sut.GetProfileAsync(UserA);

        await act.Should().ThrowAsync<ProfileNotFoundException>();
    }

    #endregion

    #region GetMyAccountAsync — subscription merge + fail-soft

    [Fact]
    public async Task GetMyAccount_MergesActiveSubscription()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);
        var endDate = DateTime.UtcNow.AddDays(20);
        var sub = Substitute.For<ISubscriptionStatusClient>();
        sub.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new SubscriptionStatusResult(true, "VIP Monthly", endDate));

        var sut = Build(db, subscription: sub);
        var result = await sut.GetMyAccountAsync(UserA, "token");

        result.HasActiveSubscription.Should().BeTrue();
        result.SubscriptionPlanName.Should().Be("VIP Monthly");
        result.SubscriptionEndDate.Should().Be(endDate);
    }

    [Fact]
    public async Task GetMyAccount_SubscriptionThrows_FailsSoft()
    {
        var db = TestUserDbContext.Create();
        SeedProfile(db, UserA);
        var sub = Substitute.For<ISubscriptionStatusClient>();
        sub.GetCurrentSubscriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Throws(new HttpRequestException("subscription down"));

        var sut = Build(db, subscription: sub);
        var result = await sut.GetMyAccountAsync(UserA, null);

        result.HasActiveSubscription.Should().BeFalse();
        result.SubscriptionPlanName.Should().BeNull();
        result.SubscriptionEndDate.Should().BeNull();
    }

    #endregion
}