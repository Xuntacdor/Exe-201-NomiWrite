namespace NomiWrite.Learning.Application.Exceptions;

public class QuizGenerationSourceNotFoundException : Exception
{
    public QuizGenerationSourceNotFoundException(Guid submissionId)
        : base($"No graded feedback was found for submission '{submissionId}'. " +
               "Submit and grade the writing first, then generate a quiz.") { }

    public QuizGenerationSourceNotFoundException(string reason)
        : base(reason) { }
}