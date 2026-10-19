using System.Collections.Concurrent;
using NomiWrite.Writing.Application.Interfaces;

namespace NomiWrite.Writing.Infrastructure.Services;

/// <summary>
/// Process-local, in-memory daily quota. Intentionally simple: it resets on
/// restart and is per-replica. Swap for a Redis/DB-backed implementation if
/// durable, cross-replica accounting is ever required.
/// </summary>
public class InMemoryAutocompleteQuotaTracker : IAutocompleteQuotaTracker
{
    // Guard against unbounded growth if the service runs for a very long time
    // without a restart: prune buckets that are not from today's UTC date.
    private const int PruneThreshold = 10_000;

    private readonly ConcurrentDictionary<string, int> _counters = new();

    public bool TryConsume(Guid userId, int dailyLimit)
    {
        if (dailyLimit <= 0)
            return false;

        if (_counters.Count > PruneThreshold)
            RemoveStaleEntries();

        var key = $"{userId:N}:{DateTime.UtcNow:yyyyMMdd}";
        var updated = _counters.AddOrUpdate(key, 1, (_, current) => current + 1);

        return updated <= dailyLimit;
    }

    private void RemoveStaleEntries()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        foreach (var key in _counters.Keys)
        {
            if (!key.EndsWith(today, StringComparison.Ordinal))
                _counters.TryRemove(key, out _);
        }
    }
}
