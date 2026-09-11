using FluentValidation;
using NomiWrite.Auth.Application.DTOs;

namespace NomiWrite.Auth.Application.Validation;

public class UpdateUserStatusRequestValidator : AbstractValidator<UpdateUserStatusRequestDto>
{
    public UpdateUserStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status is not a valid account status.");
    }
}