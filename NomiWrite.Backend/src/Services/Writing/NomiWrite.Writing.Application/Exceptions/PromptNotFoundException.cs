namespace NomiWrite.Writing.Application.Exceptions;

public class PromptNotFoundException : Exception
{
    public PromptNotFoundException(Guid promptId)
        : base($"Writing prompt with id '{promptId}' was not found.")
    {
    }
}