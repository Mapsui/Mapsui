using System.Threading.Tasks;
using Mapsui.Utilities;
using NUnit.Framework;

namespace Mapsui.Tests.Utilities;

[TestFixture]
public class StateHandoverTests
{
    private sealed record Counter(int Value, int Other);

    [Test]
    public void InitialStateCountsAsApplied()
    {
        // Arrange
        var handover = new StateHandover<Counter>(new Counter(0, 0));

        // Act & Assert
        Assert.That(handover.TryTake(out _, out _), Is.False);
        Assert.That(handover.Applied, Is.SameAs(handover.Latest));
    }

    [Test]
    public void UpdateReturnsPreviousAndLatestAndTakeSeesBoth()
    {
        // Arrange
        var handover = new StateHandover<Counter>(new Counter(0, 0));

        // Act
        var (previous, latest) = handover.Update(s => s with { Value = 1 });
        var taken = handover.TryTake(out var takenPrevious, out var takenLatest);

        // Assert
        Assert.That(previous.Value, Is.EqualTo(0));
        Assert.That(latest.Value, Is.EqualTo(1));
        Assert.That(taken, Is.True);
        Assert.That(takenPrevious, Is.SameAs(previous));
        Assert.That(takenLatest, Is.SameAs(latest));
        Assert.That(handover.TryTake(out _, out _), Is.False, "Taking twice without a new update yields nothing.");
    }

    [Test]
    public void ReturningTheSameStateIsANoOp()
    {
        // Arrange
        var handover = new StateHandover<Counter>(new Counter(0, 0));

        // Act
        var (previous, latest) = handover.Update(s => s);

        // Assert
        Assert.That(latest, Is.SameAs(previous));
        Assert.That(handover.TryTake(out _, out _), Is.False);
    }

    [Test]
    public void SeveralUpdatesCollapseIntoOneTake()
    {
        // Arrange
        var handover = new StateHandover<Counter>(new Counter(0, 0));

        // Act
        handover.Update(s => s with { Value = 1 });
        handover.Update(s => s with { Value = 2 });
        handover.TryTake(out var previous, out var latest);

        // Assert: previous is what was applied last, not the intermediate state.
        Assert.That(previous.Value, Is.EqualTo(0));
        Assert.That(latest.Value, Is.EqualTo(2));
    }

    [Test]
    public void ConcurrentUpdatesAreNeverLost()
    {
        // Arrange
        var handover = new StateHandover<Counter>(new Counter(0, 0));
        const int count = 10_000;

        // Act: two fields incremented from many threads; a lost update would leave one of them short.
        Parallel.For(0, count, i =>
        {
            if (i % 2 == 0)
                handover.Update(s => s with { Value = s.Value + 1 });
            else
                handover.Update(s => s with { Other = s.Other + 1 });
        });

        // Assert
        Assert.That(handover.Latest.Value, Is.EqualTo(count / 2));
        Assert.That(handover.Latest.Other, Is.EqualTo(count / 2));
    }
}
