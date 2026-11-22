using Mapsui.Animations;
using NUnit.Framework;

namespace Mapsui.Tests.Animations;

[TestFixture]
public class AnimationListTests
{
    [Test]
    public void FinishedEntriesAreRemovedAndTheirFinalRunsOnce()
    {
        // Arrange
        var animations = new AnimationList<object>();
        var target = new object();
        var finalCalls = 0;
        var entry = new AnimationEntry<object>(0, 1, 0, 1,
            tick: (t, e, v) => new AnimationResult<object>(t, true),
            final: (t, e) => { finalCalls++; return new AnimationResult<object>(t, false); });
        animations.Start(entry, duration: 0); // Finishes on the first update.

        // Act
        var firstUpdate = animations.Update(target);
        var secondUpdate = animations.Update(target);

        // Assert
        Assert.That(animations.Count, Is.EqualTo(0));
        Assert.That(finalCalls, Is.EqualTo(1));
        Assert.That(firstUpdate, Is.False);
        Assert.That(secondUpdate, Is.False);
    }

    [Test]
    public void StopRemovesTheEntryAndCallsFinal()
    {
        // Arrange
        var animations = new AnimationList<object>();
        var target = new object();
        var finalCalls = 0;
        var entry = new AnimationEntry<object>(0, 1, 0, 1,
            tick: (t, e, v) => new AnimationResult<object>(t, true),
            final: (t, e) => { finalCalls++; return new AnimationResult<object>(t, false); });
        animations.Start(entry, duration: 10_000);

        // Act
        animations.Stop(target, entry);
        animations.Stop(target, entry); // Stopping again is a no-op.
        animations.Stop(target, null);

        // Assert
        Assert.That(animations.Count, Is.EqualTo(0));
        Assert.That(finalCalls, Is.EqualTo(1));
    }

    [Test]
    public void RunningEntryReportsRunning()
    {
        // Arrange
        var animations = new AnimationList<object>();
        var entry = new AnimationEntry<object>(0, 1, 0, 1,
            tick: (t, e, v) => new AnimationResult<object>(t, true),
            final: (t, e) => new AnimationResult<object>(t, false));
        animations.Start(entry, duration: 10_000);

        // Act & Assert
        Assert.That(animations.Update(new object()), Is.True);
        Assert.That(animations.Count, Is.EqualTo(1));
    }
}
