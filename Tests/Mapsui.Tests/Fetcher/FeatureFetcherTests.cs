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
    public async Task TestFeatureFetcherDelayAsync()
    {
        // arrange
        var extent = new MRect(0, 0, 10, 10);
        using var layer = new Layer
        {
            DataSource = new MemoryProvider(GenerateRandomPoints(extent, 25))
        };
        layer.Delayer.MillisecondsToWait = 0;

        var notifications = new List<bool>();
        layer.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Layer.Busy))
            {
                notifications.Add(layer.Busy);
            }
        };
        var fetchInfo = new FetchInfo(extent, 1, null, ChangeType.Discrete);

        // act
        layer.RefreshData(fetchInfo);

        // assert
        await Task.Run(() =>
        {
            while (notifications.Count < 2)
            {
                // just wait until we have two
            }
        });
        Assert.IsTrue(notifications[0]);
        Assert.IsFalse(notifications[1]);
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
