using FluentValidation;
using NomiWrite.Auth.Application.DTOs;

namespace NomiWrite.Auth.Application.Validation;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequestDto>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}