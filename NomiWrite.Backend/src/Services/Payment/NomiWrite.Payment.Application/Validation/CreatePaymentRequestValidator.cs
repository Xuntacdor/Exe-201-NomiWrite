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
            .Must(p => p is Domain.Enums.PaymentProvider.VNPay or Domain.Enums.PaymentProvider.Momo)
            .WithMessage("Only VNPay and MoMo checkout are supported.");

        RuleFor(x => x.PlanId)
            .NotNull().WithMessage("A subscription plan is required.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must be a 3-letter ISO code.");
    }
}
