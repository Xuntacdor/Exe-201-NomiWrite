using FluentAssertions;
using MassTransit;
using NSubstitute;
using NomiWrite.Shared.Contracts.Events.Subscription;
using NomiWrite.Subscription.Application.DTOs;
using NomiWrite.Subscription.Application.Exceptions;
using NomiWrite.Subscription.Application.Interfaces;
using NomiWrite.Subscription.Application.Services;
using NomiWrite.Subscription.Application.UnitTests.Persistence;
using NomiWrite.Subscription.Domain.Entities;
using NomiWrite.Subscription.Domain.Enums;
using NomiWrite.Subscription.Infrastructure.Persistence;

namespace NomiWrite.Subscription.Application.UnitTests;

public class SubscriptionServiceTests
{
    private static readonly Guid UserA = Guid.NewGuid();

    private static SubscriptionService Build(SubscriptionDbContext db, IPublishEndpoint? publish = null)
    {
        publish ??= Substitute.For<IPublishEndpoint>();
        return new SubscriptionService(db, publish);
    }

    private static SubscriptionPlan SeedPlan(SubscriptionDbContext db, int durationDays = 30)
    {
        var plan = new SubscriptionPlan
        {
            Name = $"Plan {Guid.NewGuid():N}",
            Description = "desc",
            Price = 99000,
            Currency = "VND",
            BillingCycle = BillingCycle.Monthly,
            DurationDays = durationDays,
            IsActive = true,
            FeaturesJson = "[]",
            CreatedAt = DateTime.UtcNow
        };
        db.SubscriptionPlans.Add(plan);
        db.SaveChanges();
        return plan;
    }

    private static UserSubscription SeedActive(SubscriptionDbContext db, Guid userId, SubscriptionPlan plan,
        DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        var sub = new UserSubscription
        {
            UserId = userId,
            PlanId = plan.Id,
            PaymentOrderId = Guid.NewGuid(),
            StartDate = now.AddDays(-10),
            EndDate = endDate ?? now.AddDays(20),
            Status = SubscriptionStatus.Active,
            CreatedAt = now
        };
        db.UserSubscriptions.Add(sub);
        db.SaveChanges();
        return sub;
    }

    private static Guid OnlyOrderId(SubscriptionDbContext db) => db.UserSubscriptions.Single().PaymentOrderId;

    #region U-S1 — ActivateSubscriptionFromPaymentAsync (create/switch/extend)

    [Fact]
    public async Task Activate_NoActiveSubscription_CreatesNew()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db, durationDays: 30);
        var publish = Substitute.For<IPublishEndpoint>();
        var sut = Build(db, publish);
        var orderId = Guid.NewGuid();

        await sut.ActivateSubscriptionFromPaymentAsync(UserA, plan.Id, orderId);

        var stored = db.UserSubscriptions.Single();
        stored.UserId.Should().Be(UserA);
        stored.PlanId.Should().Be(plan.Id);
        stored.PaymentOrderId.Should().Be(orderId);
        stored.Status.Should().Be(SubscriptionStatus.Active);
        stored.EndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));

        var evt = publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments())
            .OfType<SubscriptionActivatedEvent>()
            .Single();
        evt.UserId.Should().Be(UserA);
        evt.PlanId.Should().Be(plan.Id);
        evt.EndDate.Should().Be(stored.EndDate);
    }

    [Fact]
    public async Task Activate_DifferentPlan_SwitchesPlanNotExtends()
    {
        var db = TestSubscriptionDbContext.Create();
        var planA = SeedPlan(db, durationDays: 30);
        var planB = SeedPlan(db, durationDays: 60);
        var sut = Build(db);
        await sut.ActivateSubscriptionFromPaymentAsync(UserA, planA.Id, Guid.NewGuid());
        var before = DateTime.UtcNow;

        await sut.ActivateSubscriptionFromPaymentAsync(UserA, planB.Id, Guid.NewGuid());

        var stored = db.UserSubscriptions.Single();
        stored.PlanId.Should().Be(planB.Id);
        stored.StartDate.Should().BeOnOrAfter(before);
        stored.EndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(60), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Activate_SamePlan_ExtendsByDuration()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db, durationDays: 30);
        var sut = Build(db);
        await sut.ActivateSubscriptionFromPaymentAsync(UserA, plan.Id, Guid.NewGuid());
        var firstEnd = db.UserSubscriptions.Single().EndDate;

        await sut.ActivateSubscriptionFromPaymentAsync(UserA, plan.Id, Guid.NewGuid());

        var stored = db.UserSubscriptions.Single();
        stored.EndDate.Should().Be(firstEnd.AddDays(30));
        db.UserSubscriptions.Should().HaveCount(1);
    }

    #endregion

    #region U-S2 — Payment-activation idempotency (documents current behavior)

    [Fact]
    public async Task Activate_SamePlan_SamePaymentOrder_DoesNotExtendAgain()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db, durationDays: 30);
        var sut = Build(db);
        var orderId = Guid.NewGuid();
        await sut.ActivateSubscriptionFromPaymentAsync(UserA, plan.Id, orderId);
        var firstEnd = db.UserSubscriptions.Single().EndDate;

        await sut.ActivateSubscriptionFromPaymentAsync(UserA, plan.Id, orderId);

        db.UserSubscriptions.Single().EndDate.Should().Be(firstEnd);
        db.ProcessedPayments.Should().ContainSingle();
    }

    [Fact]
    public async Task Activate_OlderPaymentReplayAfterRenewal_DoesNotExtendAgain()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db);
        var publish = Substitute.For<IPublishEndpoint>();
        var sut = Build(db, publish);
        var firstPayment = Guid.NewGuid();
        await sut.ActivateSubscriptionFromPaymentAsync(UserA, plan.Id, firstPayment);
        await sut.ActivateSubscriptionFromPaymentAsync(UserA, plan.Id, Guid.NewGuid());
        var endDate = db.UserSubscriptions.Single().EndDate;

        await sut.ActivateSubscriptionFromPaymentAsync(UserA, plan.Id, firstPayment);

        db.UserSubscriptions.Single().EndDate.Should().Be(endDate);
        db.ProcessedPayments.Should().HaveCount(2);
        publish.ReceivedCalls().SelectMany(c => c.GetArguments())
            .OfType<SubscriptionActivatedEvent>().Should().HaveCount(2);
    }

    [Fact]
    public async Task Activate_PromoCode_IncrementsOnlyOnceForDuplicatePayment()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db);
        db.PromoCodes.Add(new PromoCode
        {
            Code = "SAVE10",
            DiscountPercent = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
        var sut = Build(db);
        var paymentId = Guid.NewGuid();

        await sut.ActivateSubscriptionFromPaymentAsync(UserA, plan.Id, paymentId, "save10");
        await sut.ActivateSubscriptionFromPaymentAsync(UserA, plan.Id, paymentId, "save10");

        db.PromoCodes.Single().TimesRedeemed.Should().Be(1);
    }

    #endregion

    #region U-S3 — Promo validation (current behavior documented)

    [Fact]
    public async Task ValidatePromo_UnknownCode_Invalid()
    {
        var db = TestSubscriptionDbContext.Create();

        var sut = Build(db);
        var result = await sut.ValidatePromoCodeAsync("NOPE-123");

        result.Valid.Should().BeFalse();
        result.DiscountPercent.Should().BeNull();
    }

    [Fact]
    public async Task ValidatePromo_LookupIsCaseInsensitiveAndTrimmed()
    {
        var db = TestSubscriptionDbContext.Create();
        db.PromoCodes.Add(new PromoCode
        {
            Code = "SAVE20",
            DiscountPercent = 20,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            MaxRedemptions = 100,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        var sut = Build(db);
        var result = await sut.ValidatePromoCodeAsync("  save20  ");

        result.Valid.Should().BeTrue();
        result.DiscountPercent.Should().Be(20);
    }

    [Fact]
    public async Task ValidatePromo_ExpiredCode_Invalid()
    {
        var db = TestSubscriptionDbContext.Create();
        db.PromoCodes.Add(new PromoCode
        {
            Code = "OLD",
            DiscountPercent = 10,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        var sut = Build(db);
        var result = await sut.ValidatePromoCodeAsync("OLD");

        result.Valid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePromo_NotExpiring_ConsideredActive()
    {
        var db = TestSubscriptionDbContext.Create();
        db.PromoCodes.Add(new PromoCode
        {
            Code = "NOEXP",
            DiscountPercent = 15,
            IsActive = true,
            ExpiresAt = null,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        var sut = Build(db);
        var result = await sut.ValidatePromoCodeAsync("NOEXP");

        result.Valid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePromo_MaxRedemptionsReached_Invalid()
    {
        var db = TestSubscriptionDbContext.Create();
        db.PromoCodes.Add(new PromoCode
        {
            Code = "LIMITED",
            DiscountPercent = 5,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            MaxRedemptions = 50,
            TimesRedeemed = 50,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        var sut = Build(db);
        var result = await sut.ValidatePromoCodeAsync("LIMITED");

        result.Valid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePromo_Inactive_Invalid()
    {
        var db = TestSubscriptionDbContext.Create();
        db.PromoCodes.Add(new PromoCode
        {
            Code = "DISABLED",
            DiscountPercent = 5,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        var sut = Build(db);
        var result = await sut.ValidatePromoCodeAsync("DISABLED");

        result.Valid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePromo_Whitespace_Invalid()
    {
        var db = TestSubscriptionDbContext.Create();

        var sut = Build(db);
        var result = await sut.ValidatePromoCodeAsync("   ");

        result.Valid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePromo_ValidCode_DoesNotIncrementTimesRedeemed()
    {
        var db = TestSubscriptionDbContext.Create();
        db.PromoCodes.Add(new PromoCode
        {
            Code = "SAVE10",
            DiscountPercent = 10,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            MaxRedemptions = 10,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        var sut = Build(db);
        await sut.ValidatePromoCodeAsync("SAVE10");

        db.PromoCodes.Single().TimesRedeemed.Should().Be(0);
    }

    #endregion

    #region U-S5 — Cancel-subscription rules

    [Fact]
    public async Task Cancel_ActiveSubscription_CancelsImmediately()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db);
        SeedActive(db, UserA, plan);

        var sut = Build(db);
        var result = await sut.CancelSubscriptionAsync(UserA);

        result.Status.Should().Be(SubscriptionStatus.Cancelled);
        result.DaysRemaining.Should().Be(0);
        result.EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var stored = db.UserSubscriptions.Single();
        stored.Status.Should().Be(SubscriptionStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_NoActiveSubscription_Throws()
    {
        var db = TestSubscriptionDbContext.Create();

        var sut = Build(db);
        var act = () => sut.CancelSubscriptionAsync(UserA);

        await act.Should().ThrowAsync<NoActiveSubscriptionException>();
    }

    [Fact]
    public async Task Cancel_AlreadyExpired_Throws()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db);
        SeedActive(db, UserA, plan, endDate: DateTime.UtcNow.AddDays(-1));

        var sut = Build(db);
        var act = () => sut.CancelSubscriptionAsync(UserA);

        await act.Should().ThrowAsync<NoActiveSubscriptionException>();
    }

    #endregion

    #region U-S6 — GetCurrentSubscriptionAsync days-remaining math

    [Fact]
    public async Task GetCurrent_ActiveFutureSubscription_ComputesDaysRemaining()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db);
        SeedActive(db, UserA, plan, endDate: DateTime.UtcNow.AddDays(10).AddHours(1));

        var sut = Build(db);
        var result = await sut.GetCurrentSubscriptionAsync(UserA);

        result.Should().NotBeNull();
        result!.Status.Should().Be(SubscriptionStatus.Active);
        result.DaysRemaining.Should().Be(10);
        result.PlanName.Should().Be(plan.Name);
    }

    [Fact]
    public async Task GetCurrent_ExpiredSubscription_ReportsExpiredWithZeroDays()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db);
        SeedActive(db, UserA, plan, endDate: DateTime.UtcNow.AddDays(-2));

        var sut = Build(db);
        var result = await sut.GetCurrentSubscriptionAsync(UserA);

        result.Should().NotBeNull();
        result!.Status.Should().Be(SubscriptionStatus.Expired);
        result.DaysRemaining.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrent_NoSubscription_ReturnsNull()
    {
        var db = TestSubscriptionDbContext.Create();

        var sut = Build(db);
        var result = await sut.GetCurrentSubscriptionAsync(UserA);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrent_OnlyConsidersActiveStatus()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db);
        var cancelled = SeedActive(db, UserA, plan);
        cancelled.Status = SubscriptionStatus.Cancelled;
        db.SaveChanges();

        var sut = Build(db);
        var result = await sut.GetCurrentSubscriptionAsync(UserA);

        result.Should().BeNull();
    }

    #endregion
}
