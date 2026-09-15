using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.Shared.Contracts.Events.Grading;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Infrastructure.Consumers;

public class GradingCompletedEventConsumer : IConsumer<GradingCompletedEvent>
{
    private readonly IWritingDbContext _dbContext;
    private readonly ILogger<GradingCompletedEventConsumer> _logger;

    public GradingCompletedEventConsumer(
        IWritingDbContext dbContext,
        ILogger<GradingCompletedEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GradingCompletedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Received GradingCompletedEvent for submission {SubmissionId}.",
            @event.SubmissionId);

        var submission = await _dbContext.WritingSubmissions
            .FirstOrDefaultAsync(s => s.Id == @event.SubmissionId);

        if (submission is null)
        {
            _logger.LogWarning(
                "Submission {SubmissionId} not found; skipping grading completion.",
                @event.SubmissionId);
            return;
        }

        if (submission.GradedAt.HasValue)
        {
            _logger.LogDebug(
                "Submission {SubmissionId} is already graded; skipping.",
                @event.SubmissionId);
            return;
        }

        submission.Status = SubmissionStatus.Submitted;
        submission.GradedAt = @event.CompletedAt;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Marked submission {SubmissionId} as graded.",
            @event.SubmissionId);
    }
}