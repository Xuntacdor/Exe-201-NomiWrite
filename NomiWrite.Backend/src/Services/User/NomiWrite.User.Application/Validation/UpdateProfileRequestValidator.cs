using FluentValidation;
using NomiWrite.User.Application.DTOs;

namespace NomiWrite.User.Application.Validation;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequestDto>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(100)
            .When(x => x.DisplayName is not null);

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => x.Bio is not null);

        RuleFor(x => x.TargetBand)
            .InclusiveBetween(0, 9)
            .When(x => x.TargetBand is not null);
    }
}