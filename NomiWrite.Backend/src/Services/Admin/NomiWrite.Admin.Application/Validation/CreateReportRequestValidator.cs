using FluentValidation;
using NomiWrite.Admin.Application.DTOs;

namespace NomiWrite.Admin.Application.Validation;

public class CreateReportRequestValidator : AbstractValidator<CreateReportRequestDto>
{
    public CreateReportRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters.");
    }
}
