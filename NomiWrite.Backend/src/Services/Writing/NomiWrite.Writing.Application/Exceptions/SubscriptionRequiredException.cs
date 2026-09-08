namespace NomiWrite.Writing.Application.Exceptions;

public class SubscriptionRequiredException : Exception
{
    public SubscriptionRequiredException()
        : base("Sample answers are a VIP feature. Subscribe to unlock this content.")
    {
    }
}