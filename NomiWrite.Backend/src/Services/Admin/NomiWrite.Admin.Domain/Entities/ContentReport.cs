using NomiWrite.Admin.Domain.Common;
using NomiWrite.Admin.Domain.Enums;

namespace NomiWrite.Admin.Domain.Entities;

public class ContentReport : BaseEntity
{
    public Guid ReporterUserId { get; set; }
    public ContentType ContentType { get; set; }
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? ModeratorNotes { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
