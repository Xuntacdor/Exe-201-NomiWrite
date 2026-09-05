using FluentValidation;
using NomiWrite.Payment.Application.DTOs;

namespace NomiWrite.Payment.Application.Validation;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequestDto>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Provider)
            .IsInEnum().WithMessage("Provider is not a supported payment provider.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must be a 3-letter ISO code.");
    }
}