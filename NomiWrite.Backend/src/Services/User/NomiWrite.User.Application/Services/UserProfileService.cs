using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.User.Application.DTOs;
using NomiWrite.User.Application.Exceptions;
using NomiWrite.User.Application.Interfaces;
using NomiWrite.User.Domain.Entities;

namespace NomiWrite.User.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserDbContext _dbContext;
    private readonly IValidator<UpdateProfileRequestDto> _updateProfileValidator;
    private readonly ISubscriptionStatusClient _subscriptionStatusClient;
    private readonly IWritingHistoryClient _writingHistoryClient;
    private readonly IGradingHistoryClient _gradingHistoryClient;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IUserDbContext dbContext,
        IValidator<UpdateProfileRequestDto> updateProfileValidator,
        ISubscriptionStatusClient subscriptionStatusClient,
        IWritingHistoryClient writingHistoryClient,
        IGradingHistoryClient gradingHistoryClient,
        ILogger<UserProfileService> logger)
    {
        _dbContext = dbContext;
        _updateProfileValidator = updateProfileValidator;
        _subscriptionStatusClient = subscriptionStatusClient;
        _writingHistoryClient = writingHistoryClient;
        _gradingHistoryClient = gradingHistoryClient;
        _logger = logger;
    }

    public async Task CreateProfileFromRegistrationAsync(Guid userId, string fullName)
    {
        var existing = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing is not null)
            return;

        var now = DateTime.UtcNow;
        _dbContext.UserProfiles.Add(new UserProfile
        {
            UserId = userId,
            DisplayName = fullName,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _dbContext.SaveChangesAsync();
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ProfileNotFoundException(userId);

        return ToProfileDto(profile);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto dto)
    {
        var validationResult = await _updateProfileValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ProfileNotFoundException(userId);

        if (dto.DisplayName is not null)
            profile.DisplayName = dto.DisplayName;

        if (dto.AvatarUrl is not null)
            profile.AvatarUrl = dto.AvatarUrl;

        if (dto.Bio is not null)
            profile.Bio = dto.Bio;

        if (dto.TargetExam is not null)
            profile.TargetExam = dto.TargetExam;

        if (dto.TargetBand is not null)
            profile.TargetBand = dto.TargetBand;

        if (dto.TargetExamDate is not null)
            profile.TargetExamDate = dto.TargetExamDate;

        if (dto.EnglishLevel is not null)
            profile.EnglishLevel = dto.EnglishLevel;

        profile.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ToProfileDto(profile);
    }

    public async Task<MyAccountDto> GetMyAccountAsync(Guid userId, string? accessToken)
    {
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ProfileNotFoundException(userId);

        bool hasActiveSubscription = false;
        string? subscriptionPlanName = null;
        DateTime? subscriptionEndDate = null;

        try
        {
            var subscription = await _subscriptionStatusClient.GetCurrentSubscriptionAsync(userId, accessToken);
            hasActiveSubscription = subscription.HasActiveSubscription;
            subscriptionPlanName = subscription.PlanName;
            subscriptionEndDate = subscription.EndDate;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to retrieve subscription status for user {UserId}; defaulting to no active subscription.",
                userId);
        }

        return new MyAccountDto
        {
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            AvatarUrl = profile.AvatarUrl,
            Bio = profile.Bio,
            TargetExam = profile.TargetExam,
            TargetBand = profile.TargetBand,
            TargetExamDate = profile.TargetExamDate,
            EnglishLevel = profile.EnglishLevel,
            HasActiveSubscription = hasActiveSubscription,
            SubscriptionPlanName = subscriptionPlanName,
            SubscriptionEndDate = subscriptionEndDate
        };
    }

    public async Task<ProgressResponseDto> GetProgressAsync(Guid userId, string? accessToken)
    {
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ProfileNotFoundException(userId);

        IReadOnlyList<WritingSubmissionSummary> submissions = Array.Empty<WritingSubmissionSummary>();
        IReadOnlyList<GradingHistoryEntry> gradingHistory = Array.Empty<GradingHistoryEntry>();

        try
        {
            submissions = await _writingHistoryClient.GetSubmissionsAsync(userId, accessToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to retrieve writing history for user {UserId}; defaulting to empty.",
                userId);
        }

        try
        {
            gradingHistory = await _gradingHistoryClient.GetHistoryAsync(userId, accessToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to retrieve grading history for user {UserId}; defaulting to empty.",
                userId);
        }

        var bandHistory = gradingHistory
            .OrderBy(g => g.CreatedAt)
            .Select(g => new BandHistoryPointDto
            {
                Date = g.CreatedAt,
                Band = g.OverallBand
            })
            .ToList();

        var totalSubmissions = submissions.Count;

        return new ProgressResponseDto
        {
            BandHistory = bandHistory,
            StrengthsWeaknesses = ComputeStrengthsWeaknesses(gradingHistory),
            CurrentStreak = ComputeCurrentStreak(submissions),
            TotalSubmissions = totalSubmissions,
            Badges = ComputeBadges(totalSubmissions, gradingHistory),
            TargetExam = profile.TargetExam,
            TargetBand = profile.TargetBand,
            TargetExamDate = profile.TargetExamDate
        };
    }

    private static string? ComputeStrengthsWeaknesses(IReadOnlyList<GradingHistoryEntry> gradingHistory)
    {
        var completed = gradingHistory
            .OrderByDescending(g => g.CreatedAt)
            .Where(g => g.CriteriaScores.Count > 0)
            .Take(5)
            .ToList();

        if (completed.Count == 0)
            return null;

        var averages = new Dictionary<string, decimal>();

        foreach (var entry in completed)
        {
            foreach (var (criterion, score) in entry.CriteriaScores)
            {
                if (!averages.TryGetValue(criterion, out var current))
                    averages[criterion] = 0;
                averages[criterion] = current + (score / completed.Count);
            }
        }

        if (averages.Count == 0)
            return null;

        // This is a simple heuristic based on the average score per criterion across
        // the last few completed grading results. It is NOT a sophisticated trend
        // analysis — it simply labels the highest-average criterion as a strength and
        // the lowest-average criterion as a weakness so the user gets actionable,
        // human-readable feedback without a dedicated ML/analysis step.
        var strongest = averages.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        var weakest = averages.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

        if (string.Equals(strongest, weakest, StringComparison.OrdinalIgnoreCase))
            return $"{strongest} is your strongest area.";

        return $"{strongest} is your strongest area; {weakest} needs the most improvement.";
    }

    private static int ComputeCurrentStreak(IReadOnlyList<WritingSubmissionSummary> submissions)
    {
        if (submissions.Count == 0)
            return 0;

        var today = DateTime.UtcNow.Date;
        var submissionDates = submissions
            .Select(s => s.SubmittedAt.Date)
            .Distinct()
            .ToHashSet();

        var anchor = submissionDates.Contains(today)
            ? today
            : submissionDates.Contains(today.AddDays(-1))
                ? today.AddDays(-1)
                : (DateTime?)null;

        if (anchor is null)
            return 0;

        var streak = 0;
        var cursor = anchor.Value;

        while (submissionDates.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    private static List<BadgeDto> ComputeBadges(int totalSubmissions, IReadOnlyList<GradingHistoryEntry> gradingHistory)
    {
        var highestBand = gradingHistory.Count == 0
            ? (decimal?)null
            : gradingHistory.Max(g => g.OverallBand);

        return new List<BadgeDto>
        {
            new() { Name = "First Steps", Achieved = totalSubmissions >= 1 },
            new() { Name = "Getting Started", Achieved = totalSubmissions >= 10 },
            new() { Name = "Dedicated Writer", Achieved = totalSubmissions >= 50 },
            new() { Name = "Band 7 Achiever", Achieved = highestBand >= 7.0m },
            new() { Name = "Band 8 Achiever", Achieved = highestBand >= 8.0m }
        };
    }

    private static UserProfileDto ToProfileDto(UserProfile profile)
    {
        return new UserProfileDto
        {
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            AvatarUrl = profile.AvatarUrl,
            Bio = profile.Bio,
            TargetExam = profile.TargetExam,
            TargetBand = profile.TargetBand,
            TargetExamDate = profile.TargetExamDate,
            EnglishLevel = profile.EnglishLevel
        };
    }
}