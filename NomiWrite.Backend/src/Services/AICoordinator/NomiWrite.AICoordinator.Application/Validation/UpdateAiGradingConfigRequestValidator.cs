using FluentValidation;
using NomiWrite.AICoordinator.Application.DTOs;

namespace NomiWrite.AICoordinator.Application.Validation;

public class UpdateAiGradingConfigRequestValidator : AbstractValidator<UpdateAiGradingConfigRequestDto>
{
    public UpdateAiGradingConfigRequestValidator()
    {
        RuleFor(x => x.ProviderName)
            .NotEmpty().WithMessage("ProviderName is required.")
            .MaximumLength(50).WithMessage("ProviderName must not exceed 50 characters.");

        RuleFor(x => x.ModelName)
            .NotEmpty().WithMessage("ModelName is required.")
            .MaximumLength(100).WithMessage("ModelName must not exceed 100 characters.");

        RuleFor(x => x.Temperature)
            .InclusiveBetween((decimal)0.0, (decimal)2.0).When(x => x.Temperature.HasValue)
            .WithMessage("Temperature must be between 0.0 and 2.0.");

        RuleFor(x => x.MaxOutputTokens)
            .GreaterThan(0).When(x => x.MaxOutputTokens.HasValue)
            .WithMessage("MaxOutputTokens must be greater than 0.");
    }
}