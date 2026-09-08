using NomiWrite.User.Domain.Enums;

namespace NomiWrite.User.Application.DTOs;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? TargetExam { get; set; }
    public decimal? TargetBand { get; set; }
    public DateTime? TargetExamDate { get; set; }
    public EnglishLevel? EnglishLevel { get; set; }
}

public class UpdateProfileRequestDto
{
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? TargetExam { get; set; }
    public decimal? TargetBand { get; set; }
    public DateTime? TargetExamDate { get; set; }
    public EnglishLevel? EnglishLevel { get; set; }
}

public class MyAccountDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? TargetExam { get; set; }
    public decimal? TargetBand { get; set; }
    public DateTime? TargetExamDate { get; set; }
    public EnglishLevel? EnglishLevel { get; set; }

    public bool HasActiveSubscription { get; set; }
    public string? SubscriptionPlanName { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
}

public class ProgressResponseDto
{
    public List<BandHistoryPointDto> BandHistory { get; set; } = new();
    public string? StrengthsWeaknesses { get; set; }
    public int CurrentStreak { get; set; }
    public int TotalSubmissions { get; set; }
    public List<BadgeDto> Badges { get; set; } = new();
    public string? TargetExam { get; set; }
    public decimal? TargetBand { get; set; }
    public DateTime? TargetExamDate { get; set; }
}

public class BandHistoryPointDto
{
    public DateTime Date { get; set; }
    public decimal Band { get; set; }
}

public class BadgeDto
{
    public string Name { get; set; } = string.Empty;
    public bool Achieved { get; set; }
}
