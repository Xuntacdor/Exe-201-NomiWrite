using FluentValidation;
using NomiWrite.Payment.Application.DTOs;

namespace NomiWrite.Payment.Application.Validation;

public class CreateRefundRequestValidator : AbstractValidator<CreateRefundRequestDto>
{
    public CreateRefundRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters.");
    }
}
