using FluentValidation;
using NomiWrite.Auth.Application.DTOs;

namespace NomiWrite.Auth.Application.Validation;

public class ResendVerificationEmailRequestValidator : AbstractValidator<ResendVerificationEmailRequestDto>
{
    public ResendVerificationEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}