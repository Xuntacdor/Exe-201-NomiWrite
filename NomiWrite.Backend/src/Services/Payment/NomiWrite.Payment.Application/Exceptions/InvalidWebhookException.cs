namespace NomiWrite.Payment.Application.Exceptions;

public class InvalidWebhookException : Exception
{
    public InvalidWebhookException(string reason)
        : base($"Webhook rejected: {reason}")
    {
    }
}