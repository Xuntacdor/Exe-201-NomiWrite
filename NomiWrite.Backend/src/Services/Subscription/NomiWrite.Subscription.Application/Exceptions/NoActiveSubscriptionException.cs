namespace NomiWrite.Subscription.Application.Exceptions;

public class NoActiveSubscriptionException : Exception
{
    public NoActiveSubscriptionException()
        : base("No active subscription to cancel.")
    {
    }
}
