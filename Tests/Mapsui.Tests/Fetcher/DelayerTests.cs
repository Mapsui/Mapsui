using Mapsui.Fetcher;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Mapsui.Tests.Fetcher;

[TestFixture]
public class DelayerTests
{
    [Test]
    [Repeat(1000)]
    public async Task DelayerShouldNotHangAsync()
    {
        // Arrange
        var delayer = new Delayer();
        Random random = new(43434768);
        delayer.MillisecondsBetweenCalls = 1;
        var backgroundProcesses = new BackgroundProcesses();
        backgroundProcesses.Start(random);
        int iterationCount = 10000; // Increase this value to make it hang. I tested wth 10000

        // Act
        for (var i = 0; i < iterationCount; i++)
        {
            delayer.ExecuteDelayed(() => Math.Sqrt(random.NextDouble()));
            await Task.Delay(1);
        }

        // Assert
        backgroundProcesses.Stop();
        var delayedMethodIsCalled = false;
        delayer.ExecuteDelayed(() => delayedMethodIsCalled = true);

        await Task.Delay(100); // Wait for the delayed method to be called
        Assert.That(delayedMethodIsCalled, Is.True, "The delayed method is called");
    }

    private class BackgroundProcesses
    {
        private bool _running;

        public void Start(Random random)
        {
            _running = true;
            for (int i = 0; i < 100; i++)
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
