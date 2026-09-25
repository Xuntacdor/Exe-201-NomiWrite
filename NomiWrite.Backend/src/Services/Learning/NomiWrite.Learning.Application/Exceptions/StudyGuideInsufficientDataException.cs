namespace NomiWrite.Learning.Application.Exceptions;

/// <summary>
/// Raised when a user requests a study guide but has no graded essays and no
/// locally derived weak points (grammar / vocabulary), so there is nothing to
/// analyze. Mapped to 422 by the API middleware.
/// </summary>
public class StudyGuideInsufficientDataException : Exception
{
    public StudyGuideInsufficientDataException()
        : base("Not enough writing history to generate a study guide. Submit and get feedback on at least one essay first.") { }
}