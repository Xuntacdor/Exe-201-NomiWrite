namespace NomiWrite.Payment.Application.Exceptions;

public class PaymentNotFoundException : Exception
{
    public PaymentNotFoundException(Guid paymentId)
        : base($"Payment with id '{paymentId}' was not found.")
    {
    }

    public PaymentNotFoundException(string orderReference)
        : base($"Payment with order reference '{orderReference}' was not found.")
    {
    }
}