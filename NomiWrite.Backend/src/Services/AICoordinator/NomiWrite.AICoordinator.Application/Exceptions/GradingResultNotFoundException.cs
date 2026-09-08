namespace NomiWrite.AICoordinator.Application.Exceptions;

public class GradingResultNotFoundException : Exception
{
    public GradingResultNotFoundException(Guid submissionId)
        : base($"Grading result for submission '{submissionId}' was not found.")
    {
    }
}
