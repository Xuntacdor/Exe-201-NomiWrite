using FluentValidation;
using NomiWrite.Writing.Application.DTOs;

namespace NomiWrite.Writing.Application.Validation;

public class UpdateSubmissionRequestValidator : AbstractValidator<UpdateSubmissionRequestDto>
{
    public UpdateSubmissionRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotNull().WithMessage("Content is required.")
            .MaximumLength(20000).WithMessage("Content must not exceed 20000 characters.");
    }
}