using NomiWrite.Admin.Application.DTOs;
using NomiWrite.Admin.Domain.Enums;

namespace NomiWrite.Admin.Application.Interfaces;

public interface IContentReportService
{
    Task<ContentReportDto> CreateReportAsync(Guid reporterUserId, CreateReportRequestDto request);
    Task<ContentReportListResponseDto> GetReportsAsync(ReportStatus? status, ContentType? contentType, int page, int pageSize);
    Task<ContentReportDto> ResolveReportAsync(Guid reportId, Guid moderatorUserId, ResolveReportRequestDto request);
}
