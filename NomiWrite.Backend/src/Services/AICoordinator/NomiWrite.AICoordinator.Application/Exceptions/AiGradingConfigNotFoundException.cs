namespace NomiWrite.AICoordinator.Application.Exceptions;

public class AiGradingConfigNotFoundException : Exception
{
    public AiGradingConfigNotFoundException()
        : base("No active AI grading configuration was found.")
    {
    }
}