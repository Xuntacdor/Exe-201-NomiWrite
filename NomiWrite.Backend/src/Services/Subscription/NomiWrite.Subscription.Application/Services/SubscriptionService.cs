using MassTransit;
using Microsoft.EntityFrameworkCore;
using NomiWrite.Shared.Contracts.Events.Subscription;
using NomiWrite.Subscription.Application.DTOs;
using NomiWrite.Subscription.Application.Exceptions;
using NomiWrite.Subscription.Application.Interfaces;
using NomiWrite.Subscription.Domain.Entities;
using NomiWrite.Subscription.Domain.Enums;

namespace NomiWrite.Subscription.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public SubscriptionService(ISubscriptionDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<IEnumerable<SubscriptionPlanDto>> GetActivePlansAsync()
    {
        return await _dbContext.SubscriptionPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .Select(p => new SubscriptionPlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Currency = p.Currency,
                BillingCycle = p.BillingCycle,
                DurationDays = p.DurationDays
            })
            .ToListAsync();
    }

    public async Task<UserSubscriptionStatusDto?> GetCurrentSubscriptionAsync(Guid userId)
    {
        var subscription = await _dbContext.UserSubscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();

        if (subscription is null)
            return null;

        var now = DateTime.UtcNow;

        if (subscription.EndDate < now)
        {
            return new UserSubscriptionStatusDto
            {
                PlanName = subscription.Plan?.Name ?? string.Empty,
                Status = SubscriptionStatus.Expired,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                DaysRemaining = 0
            };
        }

        return new UserSubscriptionStatusDto
        {
            PlanName = subscription.Plan?.Name ?? string.Empty,
            Status = subscription.Status,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            DaysRemaining = (subscription.EndDate - now).Days
        };
    }

    public async Task<UserSubscriptionStatusDto> CancelSubscriptionAsync(Guid userId)
    {
        var now = DateTime.UtcNow;

        var activeSubscription = await _dbContext.UserSubscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId
                && s.Status == SubscriptionStatus.Active
                && s.EndDate >= now)
            ?? throw new NoActiveSubscriptionException();

        activeSubscription.Status = SubscriptionStatus.Cancelled;
        activeSubscription.EndDate = now;
        activeSubscription.UpdatedAt = now;

        await _dbContext.SaveChangesAsync();

        return new UserSubscriptionStatusDto
        {
            PlanName = activeSubscription.Plan?.Name ?? string.Empty,
            Status = activeSubscription.Status,
            StartDate = activeSubscription.StartDate,
            EndDate = activeSubscription.EndDate,
            DaysRemaining = 0
        };
    }

    public async Task<PromoCodeValidationResultDto> ValidatePromoCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return new PromoCodeValidationResultDto { Valid = false, DiscountPercent = null };

        var promoCode = await _dbContext.PromoCodes
            .FirstOrDefaultAsync(p => p.Code == code.Trim().ToUpperInvariant());

        if (promoCode is null)
            return new PromoCodeValidationResultDto { Valid = false, DiscountPercent = null };

        var now = DateTime.UtcNow;

        var isValid = promoCode.IsActive
            && (promoCode.ExpiresAt is null || promoCode.ExpiresAt > now)
            && (promoCode.MaxRedemptions is null || promoCode.TimesRedeemed < promoCode.MaxRedemptions);

        // TimesRedeemed is intentionally NOT incremented here. It is only incremented
        // on actual successful use, which would require a callback from Payment Service
        // on payment completion. That callback is a known simplification and out of
        // scope for this pass.
        return new PromoCodeValidationResultDto
        {
            Valid = isValid,
            DiscountPercent = isValid ? promoCode.DiscountPercent : null
        };
    }

    public async Task ActivateSubscriptionFromPaymentAsync(Guid userId, Guid planId, Guid paymentOrderId)
    {
        var plan = await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == planId)
            ?? throw new PlanNotFoundException(planId);

        var now = DateTime.UtcNow;

        var activeSubscription = await _dbContext.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId
                && s.Status == SubscriptionStatus.Active
                && s.EndDate >= now);

        DateTime endDate;

        if (activeSubscription is not null && activeSubscription.PlanId != planId)
        {
            activeSubscription.PlanId = planId;
            activeSubscription.PaymentOrderId = paymentOrderId;
            activeSubscription.StartDate = now;
            activeSubscription.EndDate = now.AddDays(plan.DurationDays);
            activeSubscription.UpdatedAt = now;

            endDate = activeSubscription.EndDate;
        }
        else if (activeSubscription is not null)
        {
            endDate = activeSubscription.EndDate.AddDays(plan.DurationDays);
            activeSubscription.EndDate = endDate;
            activeSubscription.UpdatedAt = now;
        }
        else
        {
            endDate = now.AddDays(plan.DurationDays);
            _dbContext.UserSubscriptions.Add(new UserSubscription
            {
                UserId = userId,
                PlanId = planId,
                PaymentOrderId = paymentOrderId,
                StartDate = now,
                EndDate = endDate,
                Status = SubscriptionStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync();

        await _publishEndpoint.Publish(new SubscriptionActivatedEvent(userId, planId, endDate));
    }
}
