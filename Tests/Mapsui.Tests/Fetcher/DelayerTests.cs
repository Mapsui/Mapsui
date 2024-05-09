using Mapsui.Fetcher;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Mapsui.Tests.Fetcher;

[TestFixture]
public class DelayerTests
{
    [Test]
    [Repeat(10)] // Increase this value for more rigorous testing
    public async Task DelayerShouldNotHangAsync()
    {
        // Arrange
        var delayer = new Delayer();
        Random random = new(43434768);
        delayer.MillisecondsBetweenCalls = 1;
        var backgroundProcessing = new BackgroundProcessing();
        backgroundProcessing.Start(random, 100);
        int iterationCount = 10; // Increase this value for more rigorous testing

        // Act
        for (var i = 0; i < iterationCount; i++)
        {
            delayer.ExecuteDelayed(() => Math.Sqrt(random.NextDouble()));
            await Task.Delay(1);
        }

        // Assert
        backgroundProcessing.Stop();
        var delayedMethodIsCalled = false;
        delayer.ExecuteDelayed(() => delayedMethodIsCalled = true);
        await WaitUntilConditionIsTrueAsync(() => delayedMethodIsCalled, 1000).ConfigureAwait(false);
        Assert.That(delayedMethodIsCalled, Is.True, "The delayed method is called");
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
