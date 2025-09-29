using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Tiling.Extensions;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Mapsui.Tests.Layers;

[TestFixture]
public class RasterizingLayerTests
{
    [Test]
    public async Task TestFeatureFetchAsync()
    {
        // arrange
        DefaultRendererFactory.Create = () => new MapRenderer();
        using var memoryLayer = CreatePointLayer();
        using var layer = new RasterizingLayer(memoryLayer);
        var schema = new GlobalSphericalMercator();
        var box = schema.Extent.ToMRect();
        var resolution = schema.Resolutions.First().Value.UnitsPerPixel;

        Assert.That(layer.GetFeatures(box, resolution).Count(), Is.EqualTo(0));
        var fetchInfo = new FetchInfo(new MSection(box, resolution), null, ChangeType.Discrete);

        // act
        layer.ViewportChanged(fetchInfo);
        var fetchJobs = layer.GetFetchJobs(0, 8);
        foreach (var fetchJob in fetchJobs)
        {
            // This will trigger the DataChanged event
            await fetchJob.FetchFunc();
        }

        // assert
        var features = layer.GetFeatures(box, resolution);
        Assert.That(features.Count(), Is.EqualTo(1));
        Assert.That(features.First(), Is.TypeOf<RasterFeature>());
        var rasterFeature = features.OfType<RasterFeature>().First();
        Assert.That(rasterFeature.Raster!.Data.Length, Is.EqualTo(5354));
    }

    private static Layer CreatePointLayer()
    {
        var random = new Random(3);
        var features = new List<IFeature>();
        for (var i = 0; i < 100; i++)
        {
            features.Add(new GeometryFeature(
                new Point(random.Next(100000, 5000000), random.Next(100000, 5000000))));
        }
        return new Layer() { DataSource = new MemoryProvider(features) };
    }
}
