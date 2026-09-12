namespace NomiWrite.Learning.Application.Exceptions;

public class SimilarVocabSuggestionAlreadyExistsException : Exception
{
    public SimilarVocabSuggestionAlreadyExistsException()
        : base("A vocabulary suggestion with the same details already exists.") { }
}