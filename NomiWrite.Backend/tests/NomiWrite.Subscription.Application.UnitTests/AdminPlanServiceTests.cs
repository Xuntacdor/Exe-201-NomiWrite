using FluentAssertions;
using FluentValidation;
using NomiWrite.Subscription.Application.DTOs;
using NomiWrite.Subscription.Application.Exceptions;
using NomiWrite.Subscription.Application.Services;
using NomiWrite.Subscription.Application.UnitTests.Persistence;
using NomiWrite.Subscription.Application.Validation;
using NomiWrite.Subscription.Domain.Entities;
using NomiWrite.Subscription.Domain.Enums;
using NomiWrite.Subscription.Infrastructure.Persistence;

namespace NomiWrite.Subscription.Application.UnitTests;

public class AdminPlanServiceTests
{
    private static AdminPlanService Build(SubscriptionDbContext db) =>
        new(db, new CreatePlanRequestValidator(), new UpdatePlanRequestValidator());

    private static SubscriptionPlan SeedPlan(SubscriptionDbContext db, decimal price = 99000, string name = "Plan")
    {
        var plan = new SubscriptionPlan
        {
            Name = name,
            Description = "desc",
            Price = price,
            Currency = "VND",
            BillingCycle = BillingCycle.Monthly,
            DurationDays = 30,
            IsActive = true,
            FeaturesJson = "[]",
            CreatedAt = DateTime.UtcNow
        };
        db.SubscriptionPlans.Add(plan);
        db.SaveChanges();
        return plan;
    }

    private static CreatePlanRequestDto ValidCreate() => new()
    {
        Name = "  Premium Plan  ",
        Description = "  Best plan  ",
        Price = 199000,
        Currency = "VND",
        BillingCycle = BillingCycle.Monthly,
        DurationDays = 30,
        FeaturesJson = " [\"ai\"] "
    };

    [Fact]
    public async Task GetPlansAsync_ReturnsPlansOrderedByPrice()
    {
        var db = TestSubscriptionDbContext.Create();
        SeedPlan(db, price: 99000, name: "Basic");
        SeedPlan(db, price: 199000, name: "Premium");

        var sut = Build(db);
        var plans = await sut.GetPlansAsync();

        plans.Should().HaveCount(2);
        plans[0].Name.Should().Be("Basic");
        plans[1].Name.Should().Be("Premium");
    }

    [Fact]
    public async Task CreatePlanAsync_InvalidRequest_ThrowsValidation()
    {
        var db = TestSubscriptionDbContext.Create();
        var sut = Build(db);

        var act = () => sut.CreatePlanAsync(new CreatePlanRequestDto { Name = "", Price = 0 });

        await act.Should().ThrowAsync<ValidationException>();
        db.SubscriptionPlans.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePlanAsync_ValidRequest_PersistsTrimmedPlan()
    {
        var db = TestSubscriptionDbContext.Create();
        var sut = Build(db);

        var result = await sut.CreatePlanAsync(ValidCreate());

        var stored = db.SubscriptionPlans.Single();
        stored.Name.Should().Be("Premium Plan");
        stored.Description.Should().Be("Best plan");
        stored.Price.Should().Be(199000);
        stored.Currency.Should().Be("VND");
        stored.BillingCycle.Should().Be(BillingCycle.Monthly);
        stored.DurationDays.Should().Be(30);
        stored.IsActive.Should().BeTrue();
        stored.FeaturesJson.Should().Be("[\"ai\"]");
        result.Id.Should().Be(stored.Id);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePlanAsync_MissingPlan_ThrowsPlanNotFound()
    {
        var db = TestSubscriptionDbContext.Create();
        var sut = Build(db);

        var act = () => sut.UpdatePlanAsync(Guid.NewGuid(), new UpdatePlanRequestDto
        {
            Name = "x",
            Description = "y",
            Price = 1,
            Currency = "VND",
            BillingCycle = BillingCycle.Monthly,
            DurationDays = 30,
            FeaturesJson = "[]"
        });

        await act.Should().ThrowAsync<PlanNotFoundException>();
    }

    [Fact]
    public async Task UpdatePlanAsync_InvalidRequest_ThrowsValidation()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db);
        var sut = Build(db);

        var act = () => sut.UpdatePlanAsync(plan.Id, new UpdatePlanRequestDto { Price = -5 });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdatePlanAsync_ValidRequest_UpdatesPlan()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db);
        var sut = Build(db);

        var result = await sut.UpdatePlanAsync(plan.Id, new UpdatePlanRequestDto
        {
            Name = "  Pro  ",
            Description = "  More features  ",
            Price = 299000,
            Currency = "USD",
            BillingCycle = BillingCycle.Yearly,
            DurationDays = 365,
            FeaturesJson = " [\"a\",\"b\"] "
        });

        var stored = db.SubscriptionPlans.Single();
        stored.Name.Should().Be("Pro");
        stored.Description.Should().Be("More features");
        stored.Price.Should().Be(299000);
        stored.Currency.Should().Be("USD");
        stored.BillingCycle.Should().Be(BillingCycle.Yearly);
        stored.DurationDays.Should().Be(365);
        stored.FeaturesJson.Should().Be("[\"a\",\"b\"]");
        result.Id.Should().Be(plan.Id);
    }

    [Fact]
    public async Task UpdatePlanStatusAsync_MissingPlan_ThrowsPlanNotFound()
    {
        var db = TestSubscriptionDbContext.Create();
        var sut = Build(db);

        var act = () => sut.UpdatePlanStatusAsync(Guid.NewGuid(), false);

        await act.Should().ThrowAsync<PlanNotFoundException>();
    }

    [Fact]
    public async Task UpdatePlanStatusAsync_ValidId_TogglesIsActive()
    {
        var db = TestSubscriptionDbContext.Create();
        var plan = SeedPlan(db);
        var sut = Build(db);

        var result = await sut.UpdatePlanStatusAsync(plan.Id, false);

        result.IsActive.Should().BeFalse();
        db.SubscriptionPlans.Single().IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreatePlanRequestValidator_RejectsBlankNameAndZeroPrice()
    {
        var validator = new CreatePlanRequestValidator();
        var result = await validator.ValidateAsync(new CreatePlanRequestDto { Name = "", Price = 0 });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task UpdatePlanRequestValidator_RejectsBlankNameAndZeroPrice()
    {
        var validator = new UpdatePlanRequestValidator();
        var result = await validator.ValidateAsync(new UpdatePlanRequestDto { Name = "", Price = 0 });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }
}

public class AdminSubscriptionServiceTests
{
    private static AdminSubscriptionService Build(SubscriptionDbContext db) => new(db);

    private static SubscriptionPlan SeedPlan(SubscriptionDbContext db) => new()
    {
        Name = "Plan",
        Description = "desc",
        Price = 99000,
        Currency = "VND",
        BillingCycle = BillingCycle.Monthly,
        DurationDays = 30,
        IsActive = true,
        FeaturesJson = "[]",
        CreatedAt = DateTime.UtcNow
    };

    private static void SeedSubscription(SubscriptionDbContext db, Guid userId,
        DateTime start, DateTime end, SubscriptionStatus status)
    {
        var plan = SeedPlan(db);
        db.SubscriptionPlans.Add(plan);
        db.UserSubscriptions.Add(new UserSubscription
        {
            UserId = userId,
            PlanId = plan.Id,
            PaymentOrderId = Guid.NewGuid(),
            StartDate = start,
            EndDate = end,
            Status = status,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    [Fact]
    public async Task GetAnalyticsAsync_CountsDistinctActiveMembers()
    {
        var db = TestSubscriptionDbContext.Create();
        var now = DateTime.UtcNow;
        var activeUser = Guid.NewGuid();
        var expiredUser = Guid.NewGuid();
        var inactiveUser = Guid.NewGuid();

        SeedSubscription(db, activeUser, now.AddDays(-10), now.AddDays(10), SubscriptionStatus.Active);
        SeedSubscription(db, activeUser, now.AddDays(-5), now.AddDays(15), SubscriptionStatus.Active);
        SeedSubscription(db, expiredUser, now.AddDays(-20), now.AddDays(-1), SubscriptionStatus.Active);
        SeedSubscription(db, inactiveUser, now.AddDays(-10), now.AddDays(10), SubscriptionStatus.Cancelled);

        var sut = Build(db);
        var analytics = await sut.GetAnalyticsAsync();

        analytics.ActiveVipMembers.Should().Be(1);
    }

    [Fact]
    public async Task GetAnalyticsAsync_NoActiveSubscriptions_ReturnsZero()
    {
        var db = TestSubscriptionDbContext.Create();

        var sut = Build(db);
        var analytics = await sut.GetAnalyticsAsync();

        analytics.ActiveVipMembers.Should().Be(0);
    }
}