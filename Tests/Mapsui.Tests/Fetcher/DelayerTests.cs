using Mapsui.Fetcher;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Tests.Fetcher;

[TestFixture]
public class DelayerTests
{
    [Test]
    public async Task DelayerShouldNotHangAsync()
    {
        // Arrange
        using var delayer = new Delayer();
        delayer.MillisecondsToWait = 1;
        HashSet<int> delayedCalls = new();

        // Act
        for (var i = 0; i < 1000; i++)
        {
            delayer.ExecuteDelayed(() => delayedCalls.Add(i));
            await Task.Delay(10);
        }

        // Assert
        Assert.That(delayedCalls.Count, Is.EqualTo(1000));
    }
}
