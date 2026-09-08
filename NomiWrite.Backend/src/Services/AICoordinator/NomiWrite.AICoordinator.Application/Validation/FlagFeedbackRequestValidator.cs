using FluentValidation;
using NomiWrite.AICoordinator.Application.DTOs;

namespace NomiWrite.AICoordinator.Application.Validation;

public class FlagFeedbackRequestValidator : AbstractValidator<FlagGradingResultRequestDto>
{
    public FlagFeedbackRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required.")
            .MaximumLength(1000)
            .WithMessage("Reason must be at most 1000 characters.");
    }
}