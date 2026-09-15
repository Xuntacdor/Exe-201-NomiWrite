namespace NomiWrite.Learning.Application.Exceptions;

public class VocabSuggestionNotFoundException : Exception
{
    public VocabSuggestionNotFoundException(Guid id)
        : base($"Vocabulary item with id '{id}' was not found.") { }
}