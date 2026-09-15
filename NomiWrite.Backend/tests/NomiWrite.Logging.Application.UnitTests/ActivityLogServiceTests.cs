using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Logging.Application.DTOs;
using NomiWrite.Logging.Application.Interfaces;
using NomiWrite.Logging.Application.Services;
using NomiWrite.Logging.Application.Validation;

namespace NomiWrite.Logging.Application.UnitTests;

public class ActivityLogServiceTests
{
    private readonly IActivityLogRepository _repository = Substitute.For<IActivityLogRepository>();
    private readonly ActivityLogService _sut;

    public ActivityLogServiceTests()
    {
        _sut = new ActivityLogService(_repository, new ActivityLogQueryValidator(),
            NullLogger<ActivityLogService>.Instance);
    }

    private static readonly PagedResultDto<ActivityLogItemDto> EmptyPage = new();

    [Fact]
    public async Task GetPagedAsync_ValidQuery_ReturnsRepositoryResult()
    {
        _repository.GetPagedAsync(Arg.Any<ActivityLogQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(EmptyPage);

        var query = new ActivityLogQueryDto
        {
            UserId = "u-1",
            Action = "LOGIN",
            FromDate = DateTime.UtcNow.AddDays(-7),
            ToDate = DateTime.UtcNow,
            PageIndex = 2,
            PageSize = 50,
            SortBy = "timestamp",
            SortDirection = SortDirection.Desc
        };

        var result = await _sut.GetPagedAsync(query);

        result.Should().BeSameAs(EmptyPage);
        await _repository.Received(1).GetPagedAsync(
            Arg.Is<ActivityLogQueryDto>(q =>
                q.UserId == "u-1" &&
                q.Action == "LOGIN" &&
                q.FromDate == query.FromDate &&
                q.ToDate == query.ToDate &&
                q.PageIndex == 2 &&
                q.PageSize == 50 &&
                q.SortBy == "timestamp" &&
                q.SortDirection == SortDirection.Desc),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_Defaults_FulfilPaginationCriteria()
    {
        _repository.GetPagedAsync(Arg.Any<ActivityLogQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(EmptyPage);

        var result = await _sut.GetPagedAsync(new ActivityLogQueryDto());

        result.Should().BeSameAs(EmptyPage);
        await _repository.Received(1).GetPagedAsync(
            Arg.Is<ActivityLogQueryDto>(q =>
                q.PageIndex == 1 &&
                q.PageSize == 20 &&
                q.SortDirection == SortDirection.Desc),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_PageIndexZero_ThrowsValidation()
    {
        var act = () => _sut.GetPagedAsync(new ActivityLogQueryDto { PageIndex = 0 });

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().GetPagedAsync(Arg.Any<ActivityLogQueryDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_PageSizeTooLarge_ThrowsValidation()
    {
        var act = () => _sut.GetPagedAsync(new ActivityLogQueryDto { PageSize = 101 });

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().GetPagedAsync(Arg.Any<ActivityLogQueryDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_FromDateAfterToDate_ThrowsValidation()
    {
        var act = () => _sut.GetPagedAsync(new ActivityLogQueryDto
        {
            FromDate = DateTime.UtcNow,
            ToDate = DateTime.UtcNow.AddDays(-1)
        });

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().GetPagedAsync(Arg.Any<ActivityLogQueryDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_InvalidSortBy_ThrowsValidation()
    {
        var act = () => _sut.GetPagedAsync(new ActivityLogQueryDto { SortBy = "metadata" });

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().GetPagedAsync(Arg.Any<ActivityLogQueryDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_NullQuery_ThrowsArgumentNull()
    {
        var act = () => _sut.GetPagedAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
        await _repository.DidNotReceive().GetPagedAsync(Arg.Any<ActivityLogQueryDto>(), Arg.Any<CancellationToken>());
    }
}