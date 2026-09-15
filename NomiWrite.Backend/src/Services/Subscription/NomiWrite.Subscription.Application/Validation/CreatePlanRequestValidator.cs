using FluentValidation;
using NomiWrite.Subscription.Application.DTOs;

namespace NomiWrite.Subscription.Application.Validation;

public class CreatePlanRequestValidator : AbstractValidator<CreatePlanRequestDto>
{
    public CreatePlanRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required.")
            .MaximumLength(200).WithMessage("Plan name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.BillingCycle)
            .IsInEnum().WithMessage("BillingCycle is not valid.");

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("DurationDays must be at least 1.");

        RuleFor(x => x.FeaturesJson)
            .NotEmpty().WithMessage("FeaturesJson is required.");
    }
}