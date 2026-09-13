using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Shared.Contracts.Events.Payment;
using NomiWrite.Subscription.Application.Interfaces;
using NomiWrite.Subscription.Infrastructure.Consumers;

namespace NomiWrite.Subscription.Application.UnitTests;

public class PaymentCompletedEventConsumerTests
{
    private static async Task Consume(ISubscriptionService service, PaymentCompletedEvent evt)
    {
        var consumer = new PaymentCompletedEventConsumer(
            service, NullLogger<PaymentCompletedEventConsumer>.Instance);
        var context = Substitute.For<ConsumeContext<PaymentCompletedEvent>>();
        context.Message.Returns(evt);
        await consumer.Consume(context);
    }

    [Fact]
    public async Task Consume_WithPlanId_CallsActivationWithPaymentFields()
    {
        var service = Substitute.For<ISubscriptionService>();
        var paymentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        await Consume(service, new PaymentCompletedEvent(paymentId, userId, 99000, "REF-1", planId));

        await service.Received(1)
            .ActivateSubscriptionFromPaymentAsync(userId, planId, paymentId);
    }

    [Fact]
    public async Task Consume_WithoutPlanId_SkipsActivation()
    {
        var service = Substitute.For<ISubscriptionService>();

        await Consume(service, new PaymentCompletedEvent(Guid.NewGuid(), Guid.NewGuid(), 99000, "REF-2", null));

        await service.DidNotReceive()
            .ActivateSubscriptionFromPaymentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>());
    }
}