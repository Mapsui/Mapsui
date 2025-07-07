using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Providers;
using NUnit.Framework;

namespace Mapsui.Tests.Fetcher;

[TestFixture]
public class FeatureFetcherTests
{
    [Test]
    public async Task Layer_BusyProperty_ChangesDuringFeatureFetchAsync()
    {
        // Arrange
        var extent = new MRect(0, 0, 10, 10);
        using var layer = new Layer
        {
            DataSource = new MemoryProvider(GenerateRandomPoints(extent, 25))
        };

        var notifications = new List<bool>();

        layer.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Layer.Busy))
            {
                notifications.Add(layer.Busy);
            }
        };

        var fetchInfo = new FetchInfo(new MSection(extent, 1), null, ChangeType.Discrete);
        layer.ViewportChanged(fetchInfo);
        var requests = layer.GetFetchJobs(0, 8);

        // Act
        foreach (var fetchJob in requests)
        {
            // This will trigger the DataChanged event
            await fetchJob.FetchFunc().ConfigureAwait(false);
        }

        // Assert
        Assert.That(notifications.Count, Is.GreaterThan(0));
        Assert.That(notifications[0], Is.True);
        Assert.That(notifications.Count, Is.GreaterThan(1));
        Assert.That(notifications[1], Is.False);
    }

    private static List<PointFeature> GenerateRandomPoints(MRect envelope, int count)
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
