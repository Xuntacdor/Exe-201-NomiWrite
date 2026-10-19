using FluentAssertions;
using NomiWrite.Writing.Infrastructure.Services;

namespace NomiWrite.Writing.Application.UnitTests;

public class InMemoryAutocompleteQuotaTrackerTests
{
    [Fact]
    public void TryConsume_AllowsUpToLimit_ThenDenies()
    {
        var tracker = new InMemoryAutocompleteQuotaTracker();
        var user = Guid.NewGuid();

        for (var i = 0; i < 3; i++)
            tracker.TryConsume(user, 3).Should().BeTrue();

        tracker.TryConsume(user, 3).Should().BeFalse();
    }

    [Fact]
    public void TryConsume_IsIsolatedPerUser()
    {
        var tracker = new InMemoryAutocompleteQuotaTracker();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        tracker.TryConsume(userA, 1).Should().BeTrue();
        tracker.TryConsume(userA, 1).Should().BeFalse();
        tracker.TryConsume(userB, 1).Should().BeTrue();
    }

    [Fact]
    public void TryConsume_NonPositiveLimit_AlwaysDenies()
    {
        var tracker = new InMemoryAutocompleteQuotaTracker();

        tracker.TryConsume(Guid.NewGuid(), 0).Should().BeFalse();
        tracker.TryConsume(Guid.NewGuid(), -1).Should().BeFalse();
    }
}
