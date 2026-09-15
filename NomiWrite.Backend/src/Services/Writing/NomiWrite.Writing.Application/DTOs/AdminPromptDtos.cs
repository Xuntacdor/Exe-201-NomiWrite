using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Application.DTOs;

public class CreatePromptRequestDto
{
    public Guid WritingTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int? MinWords { get; set; }
    public int? MaxWords { get; set; }
    public string? ImageUrl { get; set; }
    public string? SampleAnswer { get; set; }
    public bool IsVipOnly { get; set; }
}

public class UpdatePromptRequestDto
{
    public Guid WritingTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int? MinWords { get; set; }
    public int? MaxWords { get; set; }
    public string? ImageUrl { get; set; }
    public string? SampleAnswer { get; set; }
    public bool IsVipOnly { get; set; }
}

public class AdminPromptListItemDto
{
    public Guid Id { get; set; }
    public Guid WritingTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
    public bool IsActive { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int? MinWords { get; set; }
    public int? MaxWords { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsVipOnly { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}