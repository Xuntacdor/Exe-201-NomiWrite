namespace NomiWrite.Learning.Application.Exceptions;

public class QuizNotFoundException : Exception
{
    public QuizNotFoundException(Guid id)
        : base($"Quiz with id '{id}' was not found.") { }
}