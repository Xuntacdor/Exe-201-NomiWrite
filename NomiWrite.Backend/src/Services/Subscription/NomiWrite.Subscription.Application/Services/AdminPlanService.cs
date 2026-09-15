using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NomiWrite.Subscription.Application.DTOs;
using NomiWrite.Subscription.Application.Exceptions;
using NomiWrite.Subscription.Application.Interfaces;
using NomiWrite.Subscription.Domain.Entities;

namespace NomiWrite.Subscription.Application.Services;

public class AdminPlanService : IAdminPlanService
{
    private readonly ISubscriptionDbContext _dbContext;
    private readonly IValidator<CreatePlanRequestDto> _createPlanValidator;
    private readonly IValidator<UpdatePlanRequestDto> _updatePlanValidator;

    public AdminPlanService(
        ISubscriptionDbContext dbContext,
        IValidator<CreatePlanRequestDto> createPlanValidator,
        IValidator<UpdatePlanRequestDto> updatePlanValidator)
    {
        _dbContext = dbContext;
        _createPlanValidator = createPlanValidator;
        _updatePlanValidator = updatePlanValidator;
    }

    public async Task<IReadOnlyList<AdminPlanListItemDto>> GetPlansAsync()
    {
        return await _dbContext.SubscriptionPlans
            .AsNoTracking()
            .OrderBy(p => p.Price)
            .Select(p => new AdminPlanListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Currency = p.Currency,
                BillingCycle = p.BillingCycle,
                DurationDays = p.DurationDays,
                IsActive = p.IsActive,
                FeaturesJson = p.FeaturesJson,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AdminPlanListItemDto> CreatePlanAsync(CreatePlanRequestDto request)
    {
        var validationResult = await _createPlanValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var plan = new SubscriptionPlan
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Currency = request.Currency,
            BillingCycle = request.BillingCycle,
            DurationDays = request.DurationDays,
            IsActive = true,
            FeaturesJson = request.FeaturesJson.Trim()
        };

        _dbContext.SubscriptionPlans.Add(plan);
        await _dbContext.SaveChangesAsync();

        return new AdminPlanListItemDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            Currency = plan.Currency,
            BillingCycle = plan.BillingCycle,
            DurationDays = plan.DurationDays,
            IsActive = plan.IsActive,
            FeaturesJson = plan.FeaturesJson,
            CreatedAt = plan.CreatedAt
        };
    }

    public async Task<AdminPlanListItemDto> UpdatePlanAsync(Guid id, UpdatePlanRequestDto request)
    {
        var validationResult = await _updatePlanValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var plan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new PlanNotFoundException(id);

        plan.Name = request.Name.Trim();
        plan.Description = request.Description.Trim();
        plan.Price = request.Price;
        plan.Currency = request.Currency;
        plan.BillingCycle = request.BillingCycle;
        plan.DurationDays = request.DurationDays;
        plan.FeaturesJson = request.FeaturesJson.Trim();

        await _dbContext.SaveChangesAsync();

        return new AdminPlanListItemDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            Currency = plan.Currency,
            BillingCycle = plan.BillingCycle,
            DurationDays = plan.DurationDays,
            IsActive = plan.IsActive,
            FeaturesJson = plan.FeaturesJson,
            CreatedAt = plan.CreatedAt
        };
    }

    public async Task<AdminPlanListItemDto> UpdatePlanStatusAsync(Guid id, bool isActive)
    {
        var plan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new PlanNotFoundException(id);

        plan.IsActive = isActive;
        await _dbContext.SaveChangesAsync();

        return new AdminPlanListItemDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            Currency = plan.Currency,
            BillingCycle = plan.BillingCycle,
            DurationDays = plan.DurationDays,
            IsActive = plan.IsActive,
            FeaturesJson = plan.FeaturesJson,
            CreatedAt = plan.CreatedAt
        };
    }
}