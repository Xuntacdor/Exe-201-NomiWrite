using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NomiWrite.Admin.Application.DTOs;
using NomiWrite.Admin.Application.Exceptions;
using NomiWrite.Admin.Application.Interfaces;
using NomiWrite.Admin.Domain.Enums;

namespace NomiWrite.Admin.Application.Services;

public class ContentReportService : IContentReportService
{
    private readonly IAdminDbContext _dbContext;
    private readonly IValidator<CreateReportRequestDto> _createValidator;
    private readonly IValidator<ResolveReportRequestDto> _resolveValidator;

    public ContentReportService(
        IAdminDbContext dbContext,
        IValidator<CreateReportRequestDto> createValidator,
        IValidator<ResolveReportRequestDto> resolveValidator)
    {
        _dbContext = dbContext;
        _createValidator = createValidator;
        _resolveValidator = resolveValidator;
    }

    public async Task<ContentReportDto> CreateReportAsync(Guid reporterUserId, CreateReportRequestDto request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var duplicate = await _dbContext.ContentReports
            .FirstOrDefaultAsync(r =>
                r.ReporterUserId == reporterUserId
                && r.TargetId == request.TargetId
                && r.Status == ReportStatus.Pending);

        if (duplicate is not null)
            throw new DuplicateContentReportException();

        var report = new Domain.Entities.ContentReport
        {
            ReporterUserId = reporterUserId,
            ContentType = request.ContentType,
            TargetId = request.TargetId,
            Reason = request.Reason.Trim()
        };

        _dbContext.ContentReports.Add(report);
        await _dbContext.SaveChangesAsync();

        return MapToDto(report);
    }

    public async Task<ContentReportListResponseDto> GetReportsAsync(
        ReportStatus? status, ContentType? contentType, int page, int pageSize)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _dbContext.ContentReports.AsNoTracking();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (contentType.HasValue)
            query = query.Where(r => r.ContentType == contentType.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(r => new ContentReportDto
            {
                Id = r.Id,
                ReporterUserId = r.ReporterUserId,
                ContentType = r.ContentType,
                TargetId = r.TargetId,
                Reason = r.Reason,
                Status = r.Status,
                ModeratorNotes = r.ModeratorNotes,
                ResolvedByUserId = r.ResolvedByUserId,
                ResolvedAt = r.ResolvedAt,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new ContentReportListResponseDto
        {
            Page = safePage,
            PageSize = safePageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)safePageSize),
            Items = items
        };
    }

    public async Task<ContentReportDto> ResolveReportAsync(
        Guid reportId, Guid moderatorUserId, ResolveReportRequestDto request)
    {
        var validationResult = await _resolveValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var report = await _dbContext.ContentReports
            .FirstOrDefaultAsync(r => r.Id == reportId)
            ?? throw new ContentReportNotFoundException();

        if (report.Status != ReportStatus.Pending)
            return MapToDto(report);

        report.Status = request.Action;
        report.ModeratorNotes = request.ModeratorNotes?.Trim();
        report.ResolvedByUserId = moderatorUserId;
        report.ResolvedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(report);
    }

    private static ContentReportDto MapToDto(Domain.Entities.ContentReport r) => new()
    {
        Id = r.Id,
        ReporterUserId = r.ReporterUserId,
        ContentType = r.ContentType,
        TargetId = r.TargetId,
        Reason = r.Reason,
        Status = r.Status,
        ModeratorNotes = r.ModeratorNotes,
        ResolvedByUserId = r.ResolvedByUserId,
        ResolvedAt = r.ResolvedAt,
        CreatedAt = r.CreatedAt
    };
}
