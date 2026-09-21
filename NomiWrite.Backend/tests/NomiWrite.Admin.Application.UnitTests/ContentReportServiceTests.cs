using FluentAssertions;
using FluentValidation;
using NomiWrite.Admin.Application.DTOs;
using NomiWrite.Admin.Application.Exceptions;
using NomiWrite.Admin.Application.Services;
using NomiWrite.Admin.Application.UnitTests.Persistence;
using NomiWrite.Admin.Application.Validation;
using NomiWrite.Admin.Domain.Enums;

namespace NomiWrite.Admin.Application.UnitTests;

public class ContentReportServiceTests
{
    private static readonly Guid Reporter = Guid.NewGuid();
    private static readonly Guid Moderator = Guid.NewGuid();

    private static ContentReportService Build(TestAdminDbContext db)
        => new(db, new CreateReportRequestValidator(), new ResolveReportRequestValidator());

    private static CreateReportRequestDto ReportOn(Guid target, string reason = "spam") => new()
    {
        ContentType = ContentType.EssaySubmission,
        TargetId = target,
        Reason = reason
    };

    #region U-AD3 — duplicate-pending guard

    [Fact]
    public async Task CreateReport_FirstReport_StoredAsPending()
    {
        var db = TestAdminDbContext.Create();
        var target = Guid.NewGuid();

        var sut = Build(db);
        var result = await sut.CreateReportAsync(Reporter, ReportOn(target));

        result.Status.Should().Be(ReportStatus.Pending);
        result.Reason.Should().Be("spam");
        db.ContentReports.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateReport_DuplicatePending_Throws()
    {
        var db = TestAdminDbContext.Create();
        var target = Guid.NewGuid();
        var sut = Build(db);
        await sut.CreateReportAsync(Reporter, ReportOn(target));

        var act = () => sut.CreateReportAsync(Reporter, ReportOn(target));

        await act.Should().ThrowAsync<DuplicateContentReportException>();
    }

    [Fact]
    public async Task CreateReport_SameTargetDifferentReporter_Allowed()
    {
        var db = TestAdminDbContext.Create();
        var target = Guid.NewGuid();
        var sut = Build(db);
        await sut.CreateReportAsync(Reporter, ReportOn(target));

        var other = await sut.CreateReportAsync(Guid.NewGuid(), ReportOn(target));

        other.Status.Should().Be(ReportStatus.Pending);
        db.ContentReports.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateReport_SameTargetDifferentContentType_Allowed()
    {
        var db = TestAdminDbContext.Create();
        var target = Guid.NewGuid();
        var sut = Build(db);
        await sut.CreateReportAsync(Reporter, ReportOn(target));

        var other = await sut.CreateReportAsync(Reporter, new CreateReportRequestDto
        {
            ContentType = ContentType.Post,
            TargetId = target,
            Reason = "same id but different content"
        });

        other.Status.Should().Be(ReportStatus.Pending);
        db.ContentReports.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateReport_EmptyReason_Throws()
    {
        var db = TestAdminDbContext.Create();

        var sut = Build(db);
        var act = () => sut.CreateReportAsync(Reporter, ReportOn(Guid.NewGuid(), reason: ""));

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region U-AD3 — resolve/reject flow

    [Fact]
    public async Task Resolve_PendingReport_AppliesActionAndNotes()
    {
        var db = TestAdminDbContext.Create();
        var reportId = (await Build(db).CreateReportAsync(Reporter, ReportOn(Guid.NewGuid()))).Id;

        var sut = Build(db);
        var result = await sut.ResolveReportAsync(reportId, Moderator,
            new ResolveReportRequestDto { Action = ReportStatus.ActionTaken, ModeratorNotes = "fixed" });

        result.Status.Should().Be(ReportStatus.ActionTaken);
        result.ResolvedByUserId.Should().Be(Moderator);
        result.ResolvedAt.Should().NotBeNull();
        result.ModeratorNotes.Should().Be("fixed");

        var stored = db.ContentReports.Single();
        stored.Status.Should().Be(ReportStatus.ActionTaken);
    }

    [Fact]
    public async Task Resolve_AlreadyResolved_NoOpReturnsCurrent()
    {
        var db = TestAdminDbContext.Create();
        var reportId = (await Build(db).CreateReportAsync(Reporter, ReportOn(Guid.NewGuid()))).Id;
        var sut = Build(db);
        await sut.ResolveReportAsync(reportId, Moderator,
            new ResolveReportRequestDto { Action = ReportStatus.Dismissed, ModeratorNotes = "ok" });

        var result = await sut.ResolveReportAsync(reportId, Moderator,
            new ResolveReportRequestDto { Action = ReportStatus.ActionTaken, ModeratorNotes = "override" });

        result.Status.Should().Be(ReportStatus.Dismissed);
        db.ContentReports.Single().Status.Should().Be(ReportStatus.Dismissed);
    }

    [Fact]
    public async Task Resolve_UnknownReport_Throws()
    {
        var db = TestAdminDbContext.Create();

        var sut = Build(db);
        var act = () => sut.ResolveReportAsync(Guid.NewGuid(), Moderator,
            new ResolveReportRequestDto { Action = ReportStatus.ActionTaken });

        await act.Should().ThrowAsync<ContentReportNotFoundException>();
    }

    [Fact]
    public async Task Resolve_InvalidAction_Throws()
    {
        var db = TestAdminDbContext.Create();
        var reportId = (await Build(db).CreateReportAsync(Reporter, ReportOn(Guid.NewGuid()))).Id;

        var sut = Build(db);
        var act = () => sut.ResolveReportAsync(reportId, Moderator,
            new ResolveReportRequestDto { Action = (ReportStatus)999 });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ReportAfterResolve_CanReportAgain()
    {
        var db = TestAdminDbContext.Create();
        var target = Guid.NewGuid();
        var reportId = (await Build(db).CreateReportAsync(Reporter, ReportOn(target))).Id;
        var sut = Build(db);
        await sut.ResolveReportAsync(reportId, Moderator,
            new ResolveReportRequestDto { Action = ReportStatus.ActionTaken });

        var result = await sut.CreateReportAsync(Reporter, ReportOn(target));

        result.Status.Should().Be(ReportStatus.Pending);
    }

    #endregion

    #region List filtering/paging + role gating note

    [Fact]
    public async Task GetReports_FiltersByStatusAndContentType()
    {
        var db = TestAdminDbContext.Create();
        var sut = Build(db);
        var targetA = Guid.NewGuid();
        var targetB = Guid.NewGuid();
        await sut.CreateReportAsync(Reporter, new CreateReportRequestDto
        {
            ContentType = ContentType.EssaySubmission,
            TargetId = targetA,
            Reason = "one"
        });
        await sut.CreateReportAsync(Guid.NewGuid(), new CreateReportRequestDto
        {
            ContentType = ContentType.Post,
            TargetId = targetB,
            Reason = "two"
        });

        var result = await sut.GetReportsAsync(ReportStatus.Pending, ContentType.Post, 1, 20);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(r => r.TargetId == targetB);
    }

    [Fact]
    public async Task GetReports_PaginationClamping()
    {
        var db = TestAdminDbContext.Create();
        var sut = Build(db);
        for (var i = 0; i < 30; i++)
        {
            await sut.CreateReportAsync(Guid.NewGuid(), new CreateReportRequestDto
            {
                ContentType = ContentType.Comment,
                TargetId = Guid.NewGuid(),
                Reason = $"reason {i}"
            });
        }

        var result = await sut.GetReportsAsync(null, null, 0, 500);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(30);
        result.TotalPages.Should().Be(2);
    }

    // ROLE GATING: enforced at the API layer — AnnoucementsController has
    // [Authorize(Roles = "Admin")] and ModerationController relies on the ApiGateway
    // admin claims. The service layer deliberately has no role concept; this note
    // documents that administrative handlers are unreachable for non-admin callers
    // by authorization policy, covered by the API-level integration tests in the plan.
    #endregion
}
