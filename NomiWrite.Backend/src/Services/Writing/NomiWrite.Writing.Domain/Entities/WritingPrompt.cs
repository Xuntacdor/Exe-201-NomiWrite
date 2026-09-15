using NomiWrite.Writing.Domain.Common;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Domain.Entities;

public class WritingPrompt : BaseEntity
{
    public Guid WritingTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
    public bool IsActive { get; set; } = true;
    public int? TimeLimitMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public string? SampleAnswer { get; set; }
    public int? MinWords { get; set; }
    public int? MaxWords { get; set; }
    public bool IsVipOnly { get; set; }

    public WritingType? WritingType { get; set; }
    public ICollection<WritingSubmission> Submissions { get; set; } = new List<WritingSubmission>();
}