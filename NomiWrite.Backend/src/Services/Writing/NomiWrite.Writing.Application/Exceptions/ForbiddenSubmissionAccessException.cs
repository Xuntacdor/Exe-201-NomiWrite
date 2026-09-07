namespace NomiWrite.Writing.Application.Exceptions;

public class ForbiddenSubmissionAccessException : Exception
{
    public ForbiddenSubmissionAccessException(Guid submissionId)
        : base($"Access to writing submission with id '{submissionId}' is forbidden.")
    {
    }
}