namespace NomiWrite.AICoordinator.Application.Exceptions;

public class GradingResultByIdNotFoundException : Exception
{
    public GradingResultByIdNotFoundException(Guid gradingResultId)
        : base($"Grading result '{gradingResultId}' was not found.")
    {
    }
}