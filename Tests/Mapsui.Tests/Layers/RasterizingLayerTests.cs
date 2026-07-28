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
using SkiaSharp;

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

        // Assert on the decoded image rather than on the length of the encoded bytes. The encoded
        // size depends on the PNG encoder shipped with the current Skia build, so it changes on a
        // SkiaSharp upgrade even though nothing is wrong with the rendering.
        using var bitmap = SKBitmap.Decode(rasterFeature.Raster!.Data);
        Assert.That(bitmap, Is.Not.Null, "The raster data could not be decoded as an image.");
        Assert.Multiple(() =>
        {
            Assert.That(bitmap.Width, Is.EqualTo(256));
            Assert.That(bitmap.Height, Is.EqualTo(256));
        });
        Assert.That(CountDrawnPixels(bitmap), Is.GreaterThan(0), "The rasterized tile is empty.");
    }

    private static int CountDrawnPixels(SKBitmap bitmap)
    {
        var count = 0;
        for (var x = 0; x < bitmap.Width; x++)
            for (var y = 0; y < bitmap.Height; y++)
                if (bitmap.GetPixel(x, y).Alpha != 0)
                    count++;
        return count;
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
