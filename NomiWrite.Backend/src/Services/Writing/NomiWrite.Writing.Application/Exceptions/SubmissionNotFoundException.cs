namespace NomiWrite.Writing.Application.Exceptions;

public class SubmissionNotFoundException : Exception
{
    public SubmissionNotFoundException(Guid submissionId)
        : base($"Writing submission with id '{submissionId}' was not found.")
    {
    }
}