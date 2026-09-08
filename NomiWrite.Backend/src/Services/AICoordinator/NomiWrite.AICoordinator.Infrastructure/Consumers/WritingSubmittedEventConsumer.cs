using MassTransit;
using Microsoft.Extensions.Logging;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.Shared.Contracts.Events.Writing;

namespace NomiWrite.AICoordinator.Infrastructure.Consumers;

public class WritingSubmittedEventConsumer : IConsumer<WritingSubmittedEvent>
{
    private readonly IGradingService _gradingService;
    private readonly ILogger<WritingSubmittedEventConsumer> _logger;

    public WritingSubmittedEventConsumer(IGradingService gradingService, ILogger<WritingSubmittedEventConsumer> logger)
    {
        _gradingService = gradingService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WritingSubmittedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Received WritingSubmittedEvent for submission {SubmissionId} from user {UserId}",
            message.SubmissionId, message.UserId);

        await _gradingService.GradeSubmissionAsync(
            message.SubmissionId,
            message.UserId,
            message.Content);
    }
}
