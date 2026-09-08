namespace NomiWrite.AICoordinator.Application.Exceptions;

public class TutorReviewSubscriptionRequiredException : Exception
{
    public TutorReviewSubscriptionRequiredException()
        : base("Human tutor review is a VIP feature. Subscribe to unlock this content.")
    {
    }
}