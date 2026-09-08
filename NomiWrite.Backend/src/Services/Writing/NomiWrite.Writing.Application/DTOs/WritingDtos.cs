using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Application.DTOs;

public class WritingTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public WritingTypeCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class WritingPromptDto
{
    public Guid Id { get; set; }
    public Guid WritingTypeId { get; set; }
    public string WritingTypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
}

public class WritingPromptListItemDto
{
    public Guid Id { get; set; }
    public Guid WritingTypeId { get; set; }
    public string WritingTypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
}

public class CreateSubmissionRequestDto
{
    public Guid WritingPromptId { get; set; }
    public bool IsTimed { get; set; }
}

public class UpdateSubmissionRequestDto
{
    public string Content { get; set; } = string.Empty;
}

public class SubmissionResponseDto
{
    public Guid Id { get; set; }
    public Guid WritingPromptId { get; set; }
    public string PromptTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public SubmissionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

public class SubmissionListItemDto
{
    public Guid Id { get; set; }
    public Guid WritingPromptId { get; set; }
    public string PromptTitle { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public SubmissionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
}