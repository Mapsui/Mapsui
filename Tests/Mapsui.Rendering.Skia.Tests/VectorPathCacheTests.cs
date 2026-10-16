using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture]
public class VectorPathCacheTests
{
    [Test]
    public void PolygonIsRedrawnAfterModifiedWhenIdIsStable()
    {
        // Arrange
        var feature = new FeatureWithStableId { Geometry = CreateBox(5, 45) };
        var style = new VectorStyle { Fill = new Brush(Color.Red), Outline = null };

        // Act & Assert
        AssertGeometryChangeIsRendered(feature, style, CreateBox(55, 95));
    }

    [Test]
    public void LineStringIsRedrawnAfterModifiedWhenIdIsStable()
    {
        // Arrange
        var feature = new FeatureWithStableId { Geometry = CreateVerticalLine(25) };
        var style = new VectorStyle { Line = new Pen(Color.Red, 6), Outline = null };

        // Act & Assert
        AssertGeometryChangeIsRendered(feature, style, CreateVerticalLine(75));
    }

    private static void AssertGeometryChangeIsRendered(GeometryFeature feature, IStyle style, Geometry modifiedGeometry)
    {
        using var map = new Map();
        map.Layers.Add(new MemoryLayer("Layer") { Features = [feature], Style = style });
        map.Navigator.SetSize(100, 100);
        map.Navigator.CenterOnAndZoomTo(new MPoint(50, 50), 1);
        var mapRenderer = new MapRenderer();

        using var before = Render(mapRenderer, map);
        Assert.That(IsRed(before.GetPixel(25, 50)), Is.True, "The original geometry should be rendered.");

        // Without a viewport change the path cache key only changes through the feature.
        feature.Geometry = modifiedGeometry;
        feature.Modified();

        using var after = Render(mapRenderer, map);
        Assert.That(IsRed(after.GetPixel(75, 50)), Is.True, "The modified geometry should be rendered.");
        Assert.That(IsRed(after.GetPixel(25, 50)), Is.False, "The original geometry should no longer be rendered.");
    }

    private static SKBitmap Render(MapRenderer mapRenderer, Map map)
    {
        using var stream = mapRenderer.RenderToBitmapStream(map.Navigator.Viewport, map.Layers, map.RenderService, Color.White);
        return SKBitmap.Decode(stream.ToArray());
    }

    private static bool IsRed(SKColor color) => color.Red > 200 && color.Green < 50 && color.Blue < 50;

    private static Polygon CreateBox(double minX, double maxX) => new(new LinearRing([
        new Coordinate(minX, 5), new Coordinate(maxX, 5), new Coordinate(maxX, 95), new Coordinate(minX, 95), new Coordinate(minX, 5)]));

    private static LineString CreateVerticalLine(double x) => new([new Coordinate(x, 5), new Coordinate(x, 95)]);

    /// <summary>
    /// Keeps <see cref="IFeature.Id"/> stable across <see cref="IFeature.Modified"/> while
    /// <see cref="IFeature.GenerationId"/> still changes, as allowed by the IFeature contract (see issue #3375).
    /// </summary>
    private sealed class FeatureWithStableId : GeometryFeature, IFeature
    {
        public new long Id => 42;
    }
}
