using NomiWrite.User.Domain.Common;
using NomiWrite.User.Domain.Enums;

namespace NomiWrite.User.Domain.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? TargetExam { get; set; }
    public decimal? TargetBand { get; set; }
    public EnglishLevel? EnglishLevel { get; set; }
}
