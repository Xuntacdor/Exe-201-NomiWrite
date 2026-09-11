using NomiWrite.Admin.Domain.Enums;

namespace NomiWrite.Admin.Application.DTOs;

public class CreateReportRequestDto
{
    public ContentType ContentType { get; set; }
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ContentReportDto
{
    public Guid Id { get; set; }
    public Guid ReporterUserId { get; set; }
    public ContentType ContentType { get; set; }
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ReportStatus Status { get; set; }
    public string? ModeratorNotes { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ResolveReportRequestDto
{
    public ReportStatus Action { get; set; }
    public string ModeratorNotes { get; set; } = string.Empty;
}

public class ContentReportListResponseDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<ContentReportDto> Items { get; set; } = new();
}
