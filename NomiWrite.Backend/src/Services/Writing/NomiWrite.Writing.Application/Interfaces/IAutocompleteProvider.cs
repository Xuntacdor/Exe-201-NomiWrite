namespace NomiWrite.Writing.Application.Interfaces;

/// <summary>
/// Generates a short inline continuation ("ghost text") for the learner's essay.
/// Implementations must be fail-soft: return <c>null</c> when no usable
/// suggestion can be produced. The caller treats <c>null</c> as "no suggestion",
/// so autocomplete never turns into a hard error for the writer.
/// </summary>
public interface IAutocompleteProvider
{
    Task<string?> CompleteAsync(AutocompleteContext context, CancellationToken cancellationToken);
}

/// <param name="WritingPrompt">The essay prompt (instructions) the learner is answering, if known.</param>
/// <param name="Prefix">The text the learner has typed so far (already trimmed and length-capped).</param>
public record AutocompleteContext(string WritingPrompt, string Prefix);
