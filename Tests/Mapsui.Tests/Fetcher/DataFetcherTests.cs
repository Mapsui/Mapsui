// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;
using Mapsui.Fetcher;
using NUnit.Framework;

namespace Mapsui.Tests.Fetcher;

[TestFixture]
public class DataFetcherTests
{
    [Test]
    public async Task DisposeDirectlyAfterConstructionShouldNotFaultConsumerTaskAsync()
    {
        // Arrange
        const int iterations = 1000;
        var consumerTasks = new Task[iterations];

        // Act
        for (var i = 0; i < iterations; i++)
        {
            // Issue #3373: Disposing before the consumer task started caused an ObjectDisposedException.
            using var dataFetcher = new DataFetcher(() => []);
            consumerTasks[i] = dataFetcher.ConsumerTask;
        }

        try
        {
            await Task.WhenAll(consumerTasks);
        }
        catch
        {
            // The faulted tasks are asserted below.
        }

        // Assert
        var faultedTasks = consumerTasks.Where(t => t.IsFaulted).ToList();
        Assert.That(faultedTasks, Is.Empty, faultedTasks.FirstOrDefault()?.Exception?.ToString());
    }
}
