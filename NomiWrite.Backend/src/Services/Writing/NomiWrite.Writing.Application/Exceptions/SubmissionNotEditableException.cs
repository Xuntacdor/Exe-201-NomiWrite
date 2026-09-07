namespace NomiWrite.Writing.Application.Exceptions;

public class SubmissionNotEditableException : Exception
{
    public SubmissionNotEditableException(Guid submissionId)
        : base($"Writing submission with id '{submissionId}' can no longer be edited.")
    {
    }
}