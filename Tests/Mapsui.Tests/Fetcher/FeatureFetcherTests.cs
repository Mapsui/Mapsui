using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Providers;
using NUnit.Framework;

namespace Mapsui.Tests.Fetcher;

[TestFixture]
public class FeatureFetcherTests
{
    [Test]
    [Repeat(100)]
    [CancelAfter(1000)]
    public async Task TestFeatureFetcherDelayAsync(CancellationToken token)
    {
        // Arrange
        var extent = new MRect(0, 0, 10, 10);
        using var layer = new Layer
        {
            DataSource = new MemoryProvider(GenerateRandomPoints(extent, 25))
        };
        layer.Delayer.MillisecondsBetweenCalls = 0;

        var notifications = new List<bool>();

        // Because we use weak references for the PropertyChanged event handler it can get
        // garbage collected if use a lambda directly assigned to the layer.PropertyChanged.
        // So we need to create the variable below to prevent garbage collection.
        PropertyChangedEventHandler propertyChanged = (_, args) =>
        {
            if (args.PropertyName == nameof(Layer.Busy))
            {
                notifications.Add(layer.Busy);
            }
        };

        layer.PropertyChanged += propertyChanged;
        var fetchInfo = new FetchInfo(new MSection(extent, 1), null, ChangeType.Discrete);

        // Act
        layer.RefreshData(fetchInfo);

        // Assert
        await Task.Run(async () =>
        {
            while (notifications.Count < 2 && !token.IsCancellationRequested)
            {
                // Wait until we have two notifications
                await Task.Delay(100);
            }
        }).ConfigureAwait(false);

        Assert.That(notifications.Count, Is.GreaterThan(0));
        Assert.That(notifications[0], Is.True);
        Assert.That(notifications.Count, Is.GreaterThan(1));
        Assert.That(notifications[1], Is.False);
    }

    private static IEnumerable<IFeature> GenerateRandomPoints(MRect envelope, int count)
    {
        var random = new Random(0);
        var result = new List<PointFeature>();

        for (var i = 0; i < count; i++)
        {
            result.Add(new PointFeature(new MPoint(
                random.NextDouble() * envelope.Width + envelope.Left,
                random.NextDouble() * envelope.Height + envelope.Bottom)));
        }

        return result;
    }
}
