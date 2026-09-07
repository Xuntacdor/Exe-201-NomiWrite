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
    public EnglishLevel? EnglishLevel { get; set; }
}

public class UpdateProfileRequestDto
{
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? TargetExam { get; set; }
    public decimal? TargetBand { get; set; }
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
    public EnglishLevel? EnglishLevel { get; set; }

    public bool HasActiveSubscription { get; set; }
    public string? SubscriptionPlanName { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
}
