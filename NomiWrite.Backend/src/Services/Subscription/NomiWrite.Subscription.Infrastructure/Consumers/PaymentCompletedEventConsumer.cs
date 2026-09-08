using MassTransit;
using Microsoft.Extensions.Logging;
using NomiWrite.Shared.Contracts.Events.Payment;
using NomiWrite.Subscription.Application.Interfaces;

namespace NomiWrite.Subscription.Infrastructure.Consumers;

public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<PaymentCompletedEventConsumer> _logger;

    public PaymentCompletedEventConsumer(
        ISubscriptionService subscriptionService,
        ILogger<PaymentCompletedEventConsumer> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var @event = context.Message;

        if (@event.PlanId is null)
        {
            _logger.LogDebug(
                "Payment {PaymentId} has no associated plan; skipping subscription activation.",
                @event.PaymentId);
            return;
        }

        _logger.LogInformation(
            "Activating subscription for user {UserId} on plan {PlanId} from payment {PaymentId}.",
            @event.UserId,
            @event.PlanId.Value,
            @event.PaymentId);

        await _subscriptionService.ActivateSubscriptionFromPaymentAsync(
            @event.UserId,
            @event.PlanId.Value,
            @event.PaymentId);
    }
}
