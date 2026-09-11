using FluentValidation;
using NomiWrite.Auth.Application.DTOs;

namespace NomiWrite.Auth.Application.Validation;

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequestDto>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role is not a valid role.");
    }
}