namespace NomiWrite.Writing.Application.Interfaces;

/// <summary>
/// Per-user daily quota for the (VIP-only) autocomplete feature. Keeps LLM cost
/// bounded even for subscribers, who are otherwise unlimited.
/// </summary>
public interface IAutocompleteQuotaTracker
{
    /// <summary>
    /// Attempts to consume one autocomplete request for the user for today's UTC
    /// date. Returns <c>false</c> when the daily limit is already exhausted.
    /// </summary>
    bool TryConsume(Guid userId, int dailyLimit);
}
