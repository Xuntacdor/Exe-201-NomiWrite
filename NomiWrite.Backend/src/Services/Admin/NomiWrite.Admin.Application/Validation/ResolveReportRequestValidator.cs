using FluentValidation;
using NomiWrite.Admin.Application.DTOs;

namespace NomiWrite.Admin.Application.Validation;

public class ResolveReportRequestValidator : AbstractValidator<ResolveReportRequestDto>
{
    public ResolveReportRequestValidator()
    {
        RuleFor(x => x.Action)
            .IsInEnum().WithMessage("Action must be ActionTaken or Dismissed.");

        RuleFor(x => x.ModeratorNotes)
            .MaximumLength(2000).WithMessage("ModeratorNotes must not exceed 2000 characters.");
    }
}
