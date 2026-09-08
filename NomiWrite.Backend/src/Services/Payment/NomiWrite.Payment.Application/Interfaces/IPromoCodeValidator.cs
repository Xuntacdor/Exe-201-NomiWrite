namespace NomiWrite.Payment.Application.Interfaces;

public interface IPromoCodeValidator
{
    Task<PromoCodeValidationResult> ValidateAsync(string code);
}

public record PromoCodeValidationResult(bool Valid, int? DiscountPercent);
