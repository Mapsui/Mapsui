using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Tests.Common.TestTools;
using Mapsui.Tiling.Extensions;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Mapsui.Tests.Layers;

[TestFixture]
public class RasterizingLayerTests
{
    [Test]
    public void TestTimer()
    {
        // arrange
        DefaultRendererFactory.Create = () => new MapRenderer();
        using var memoryLayer = CreatePointLayer();
        using var layer = new RasterizingLayer(memoryLayer);
        var schema = new GlobalSphericalMercator();
        var box = schema.Extent.ToMRect();
        var resolution = schema.Resolutions.First().Value.UnitsPerPixel;
        using var waitHandle = new AutoResetEvent(false);

        Assert.AreEqual(0, layer.GetFeatures(box, resolution).Count());
        layer.DataChanged += (_, _) =>
        {
            // assert
            waitHandle.Set();
        };

        var fetchInfo = new FetchInfo(box, resolution, null, ChangeType.Discrete);

        // act
        layer.RefreshData(fetchInfo);

        // assert
        waitHandle.WaitOne();
        Assert.AreEqual(layer.GetFeatures(box, resolution).Count(), 1);
    }

    private static TestLayer CreatePointLayer()
    {
        var random = new Random(3);
        var features = new List<IFeature>();
        for (var i = 0; i < 100; i++)
        {
            features.Add(new GeometryFeature(
                new Point(random.Next(100000, 5000000), random.Next(100000, 5000000))));
        }
        return new TestLayer() { DataSource = new MemoryProvider(features) };
    }
}
