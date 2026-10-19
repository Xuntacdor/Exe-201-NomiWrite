namespace NomiWrite.Writing.Application.Exceptions;

public class AutocompleteQuotaExceededException : Exception
{
    public AutocompleteQuotaExceededException()
        : base("You have reached today's autocomplete limit. Try again tomorrow.")
    {
    }
}
