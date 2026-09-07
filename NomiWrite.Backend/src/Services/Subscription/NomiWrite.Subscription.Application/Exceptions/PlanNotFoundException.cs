namespace NomiWrite.Subscription.Application.Exceptions;

public class PlanNotFoundException : Exception
{
    public PlanNotFoundException(Guid planId)
        : base($"Subscription plan with id '{planId}' was not found.")
    {
    }
}
