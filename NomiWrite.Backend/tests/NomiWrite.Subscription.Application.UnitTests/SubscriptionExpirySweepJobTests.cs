using System.Reflection;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Shared.Contracts.Events.Subscription;
using NomiWrite.Subscription.Application.UnitTests.Persistence;
using NomiWrite.Subscription.Domain.Entities;
using NomiWrite.Subscription.Domain.Enums;
using NomiWrite.Subscription.Infrastructure.Jobs;
using NomiWrite.Subscription.Infrastructure.Persistence;

namespace NomiWrite.Subscription.Application.UnitTests;

public class SubscriptionExpirySweepJobTests
{
    private sealed record SweepHarness(SubscriptionExpirySweepJob Job, SubscriptionDbContext Db, IPublishEndpoint Publish);

    private static SweepHarness Build()
    {
        var dbName = Guid.NewGuid().ToString();
        var publish = Substitute.For<IPublishEndpoint>();

        var services = new ServiceCollection();
        services.AddDbContext<SubscriptionDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddScoped(_ => publish);
        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var db = new SubscriptionDbContext(
            new DbContextOptionsBuilder<SubscriptionDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options);

        var job = new SubscriptionExpirySweepJob(scopeFactory, NullLogger<SubscriptionExpirySweepJob>.Instance);
        return new SweepHarness(job, db, publish);
    }

    private static async Task Sweep(SubscriptionExpirySweepJob job)
    {
        var method = typeof(SubscriptionExpirySweepJob).GetMethod(
            "SweepAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        await (Task)method.Invoke(job, new object[] { CancellationToken.None })!;
    }

    private static async Task<UserSubscription> ReadAsync(SubscriptionDbContext db, Guid id)
    {
        db.ChangeTracker.Clear();
        return (await db.UserSubscriptions.SingleAsync(s => s.Id == id))!;
    }

    private static UserSubscription SeedActive(SubscriptionDbContext db, DateTime endDate,
        bool warningsSent = false)
    {
        var sub = new UserSubscription
        {
            UserId = Guid.NewGuid(),
            PlanId = Guid.NewGuid(),
            PaymentOrderId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = endDate,
            Status = SubscriptionStatus.Active,
            ExpiryWarningsSent = warningsSent,
            CreatedAt = DateTime.UtcNow
        };
        db.UserSubscriptions.Add(sub);
        db.SaveChanges();
        return sub;
    }

    private static List<SubscriptionExpiringEvent> ExpiringEvents(IPublishEndpoint publish)
        => publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments())
            .OfType<SubscriptionExpiringEvent>()
            .ToList();

    private static List<SubscriptionExpiredEvent> ExpiredEvents(IPublishEndpoint publish)
        => publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments())
            .OfType<SubscriptionExpiredEvent>()
            .ToList();

    #region U-S4 — Expiry sweep job

    [Fact]
    public async Task Sweep_ExpiringWithinThreeDays_PublishesWarningAndSetsGuard()
    {
        var harness = Build();
        var sub = SeedActive(harness.Db, DateTime.UtcNow.AddDays(2));

        await Sweep(harness.Job);

        var events = ExpiringEvents(harness.Publish);
        events.Should().ContainSingle(e => e.UserId == sub.UserId);
        events.Single().PlanId.Should().Be(sub.PlanId);
        (await ReadAsync(harness.Db, sub.Id)).ExpiryWarningsSent.Should().BeTrue();
    }

    [Fact]
    public async Task Sweep_WarningAlreadySent_DoesNotRepublish()
    {
        var harness = Build();
        SeedActive(harness.Db, DateTime.UtcNow.AddDays(2), warningsSent: true);

        await Sweep(harness.Job);

        ExpiringEvents(harness.Publish).Should().BeEmpty();
    }

    [Fact]
    public async Task Sweep_JustBeyondThreshold_NoWarning()
    {
        var harness = Build();
        SeedActive(harness.Db, DateTime.UtcNow.AddDays(3).AddMinutes(1));

        await Sweep(harness.Job);

        ExpiringEvents(harness.Publish).Should().BeEmpty();
        harness.Db.UserSubscriptions.Single().ExpiryWarningsSent.Should().BeFalse();
    }

    [Fact]
    public async Task Sweep_ExpiredSubscription_FlipsStatusAndPublishesExpiredEvent()
    {
        var harness = Build();
        var sub = SeedActive(harness.Db, DateTime.UtcNow.AddDays(-1));

        await Sweep(harness.Job);

        var stored = await ReadAsync(harness.Db, sub.Id);
        stored.Status.Should().Be(SubscriptionStatus.Expired);

        var events = ExpiredEvents(harness.Publish);
        events.Should().ContainSingle(e => e.UserId == sub.UserId);
    }

    [Fact]
    public async Task Sweep_ExpiredButAlreadyExpiredStatus_NotProcessedAgain()
    {
        var harness = Build();
        var sub = SeedActive(harness.Db, DateTime.UtcNow.AddDays(-1));
        sub.Status = SubscriptionStatus.Expired;
        harness.Db.SaveChanges();

        await Sweep(harness.Job);

        ExpiredEvents(harness.Publish).Should().BeEmpty();
    }

    [Fact]
    public async Task Sweep_FarFutureSubscription_Untouched()
    {
        var harness = Build();
        var sub = SeedActive(harness.Db, DateTime.UtcNow.AddDays(30));

        await Sweep(harness.Job);

        ExpiringEvents(harness.Publish).Should().BeEmpty();
        ExpiredEvents(harness.Publish).Should().BeEmpty();
        harness.Db.UserSubscriptions.Single().Status.Should().Be(SubscriptionStatus.Active);
    }

    #endregion
}