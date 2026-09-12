namespace NomiWrite.Learning.Application.Exceptions;

public class ForbiddenLearningAccessException : Exception
{
    public ForbiddenLearningAccessException()
        : base("You do not have permission to access this resource.") { }
}