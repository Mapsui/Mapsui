using Mapsui.Fetcher;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Mapsui.Tests.Fetcher;

[TestFixture]
public class ThrottlerTests
{
    [Test]
    [Repeat(10)] // Increase this value for more rigorous testing
    public async Task DelayerShouldNotHangAsync()
    {
        // Arrange
        var throttler = new Throttler();
        Random random = new(43434768);
        var backgroundProcessing = new BackgroundProcessing();
        backgroundProcessing.Start(random, 100);
        int iterationCount = 10; // Increase this value for more rigorous testing

        // Act
        for (var i = 0; i < iterationCount; i++)
        {
            await throttler.ExecuteAsync(() => Math.Sqrt(random.NextDouble()), 1);
        }

        // Assert
        backgroundProcessing.Stop();
        var delayedMethodIsCalled = false;
        await throttler.ExecuteAsync(() => delayedMethodIsCalled = true, 1);
        await WaitUntilConditionIsTrueAsync(() => delayedMethodIsCalled, 1000).ConfigureAwait(false);
        Assert.That(delayedMethodIsCalled, Is.True, "The delayed method is called");
    }

    [Test]
    public async Task Throttler_Should_Be_GarbageCollected_When_OutOfScope_Async()
    {
        // Arrange
        WeakReference weakRef = await CreateThrottlerAndReturnWeakReferenceAsync();

        // Act
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert
        Assert.That(weakRef.IsAlive, Is.False, "Throttler instance should be garbage collected when out of scope.");
    }

    private static async Task<WeakReference> CreateThrottlerAndReturnWeakReferenceAsync()
    {
        var throttler = new Throttler();
        // Put an item on the queue to simulate normal use
        await throttler.ExecuteAsync(() => { /* no-op */ }, 1);
        return new WeakReference(throttler);
    }

    private static async Task WaitUntilConditionIsTrueAsync(Func<bool> condition, int timeoutInMilliseconds)
    {
        var delay = 10;
        await Task.Run(async () =>
        {
            var iterations = 0;
            while (!condition() && iterations < (timeoutInMilliseconds / delay))
            {
                // Wait until it is called or timeout.
                await Task.Delay(delay);
                iterations++;
            }
        }).ConfigureAwait(false);
    }

    private class BackgroundProcessing
    {
        private bool _running;

        public void Start(Random random, int numberOfBackgroundThreads)
        {
            _running = true;
            for (int i = 0; i < numberOfBackgroundThreads; i++)
                _ = Task.Run(() => DoSomethingAsync(random)); // Fire and forget
        }

        public void Stop() => _running = false;

        private async Task DoSomethingAsync(Random random)
        {
            while (_running)
            {
                Math.Sqrt(random.NextDouble());
                await Task.Delay(1);
            }
        }
    }
}
