namespace NomiWrite.AICoordinator.Application.Exceptions;

public class ForbiddenGradingResultAccessException : Exception
{
    public ForbiddenGradingResultAccessException()
        : base("Access to this grading result is forbidden.")
    {
    }
}