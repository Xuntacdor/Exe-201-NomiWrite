using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using NSubstitute;
using NomiWrite.Shared.Contracts.Events.Grading;
using NomiWrite.Writing.Application.UnitTests.Persistence;
using NomiWrite.Writing.Domain.Entities;
using NomiWrite.Writing.Domain.Enums;
using NomiWrite.Writing.Infrastructure.Consumers;

namespace NomiWrite.Writing.Application.UnitTests;

public class GradingCompletedEventConsumerTests
{
    private static readonly Guid SubmissionId = Guid.NewGuid();

    [Fact]
    public async Task Consume_SubmittedSubmission_MarksGradedAndSetsGradedAt()
    {
        await using var db = TestWritingDbContext.Create();
        var userId = Guid.NewGuid();
        db.WritingSubmissions.Add(new WritingSubmission
        {
            Id = SubmissionId,
            UserId = userId,
            WritingPromptId = Guid.NewGuid(),
            Content = "essay",
            WordCount = 1,
            Status = SubmissionStatus.Submitted,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            SubmittedAt = DateTime.UtcNow.AddMinutes(-1)
        });
        await db.SaveChangesAsync();

        var completedAt = DateTime.UtcNow;
        var consumer = new GradingCompletedEventConsumer(db, NullLogger<GradingCompletedEventConsumer>.Instance);
        await ConsumeAsync(consumer, new GradingCompletedEvent(SubmissionId, userId, 6.5m, completedAt));

        var stored = db.WritingSubmissions.Single();
        stored.Status.Should().Be(SubmissionStatus.Graded);
        stored.GradedAt.Should().Be(completedAt);
    }

    [Fact]
    public async Task Consume_AlreadyGradedSubmission_KeepsOriginalGradedAt()
    {
        await using var db = TestWritingDbContext.Create();
        var originalGradedAt = DateTime.UtcNow.AddDays(-1);
        db.WritingSubmissions.Add(new WritingSubmission
        {
            Id = SubmissionId,
            UserId = Guid.NewGuid(),
            WritingPromptId = Guid.NewGuid(),
            Content = "essay",
            WordCount = 1,
            Status = SubmissionStatus.Graded,
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            SubmittedAt = DateTime.UtcNow.AddMinutes(-5),
            GradedAt = originalGradedAt
        });
        await db.SaveChangesAsync();

        var consumer = new GradingCompletedEventConsumer(db, NullLogger<GradingCompletedEventConsumer>.Instance);
        await ConsumeAsync(consumer, new GradingCompletedEvent(SubmissionId, Guid.NewGuid(), 8.0m, DateTime.UtcNow));

        var stored = db.WritingSubmissions.Single();
        stored.Status.Should().Be(SubmissionStatus.Graded);
        stored.GradedAt.Should().Be(originalGradedAt);
    }

    private static async Task ConsumeAsync(GradingCompletedEventConsumer consumer, GradingCompletedEvent message)
    {
        var context = Substitute.For<ConsumeContext<GradingCompletedEvent>>();
        context.Message.Returns(message);
        await consumer.Consume(context);
    }
}
