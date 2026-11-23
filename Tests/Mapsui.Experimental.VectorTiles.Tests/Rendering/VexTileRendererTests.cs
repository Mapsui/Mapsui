using System.Collections.Generic;
using Mapsui.Experimental.VectorTiles.Rendering;
using Mapsui.Experimental.VectorTiles.VexTileCopies;
using NUnit.Framework;
using SkiaSharp;
using VexTile.Common.Enums;
using VexTile.Renderer.Mvt.AliFlux.Drawing;
using VectorTile = Mapsui.Experimental.VectorTiles.VexTileCopies.VectorTile;

namespace Mapsui.Experimental.VectorTiles.Tests.Rendering;

[TestFixture]
public class VexTileRendererTests
{
    // Each feature is a line starting at x = its index, so a drawn line tells which feature it was.
    private static readonly Dictionary<string, object>[] FeatureAttributes =
    [
        new() { ["class"] = "motorway" },
        new() { ["class"] = "primary" },
        new() { ["class"] = "motorway" },
        new(),
        new() { ["class"] = 5L },
        new() { ["class"] = "path", ["subclass"] = "footway" },
    ];

    [TestCase("[\"in\", \"class\", \"motorway\", \"primary\"]", new[] { 0, 1, 2 })]
    [TestCase("[\"==\", \"class\", \"motorway\"]", new[] { 0, 2 })]
    [TestCase("[\"all\", [\"==\", \"$type\", \"LineString\"], [\"in\", \"class\", \"primary\", \"motorway\"]]", new[] { 0, 1, 2 })]
    [TestCase("[\"==\", \"subclass\", \"footway\"]", new[] { 5 })]
    [TestCase("[\"in\", \"class\", \"bridge\"]", new int[0])]
    [TestCase("[\"in\", \"class\", \"motorway\", \"primary\", \"motorway\"]", new[] { 0, 1, 2 })]
    [TestCase("[\"in\", \"class\", 5]", new[] { 4 })]
    [TestCase("[\"any\", [\"==\", \"class\", \"primary\"], [\"!has\", \"class\"]]", new[] { 1, 3 })]
    public void Render_WithClassFilter_DrawsTheMatchingFeaturesInOrder(string filter, int[] expected)
    {
        var canvas = new RecordingCanvas();

        VexTileRenderer.Render(CreateTile(), CreateStyle($"\"filter\": {filter}"), canvas, 0, 0, 14);

        Assert.That(canvas.DrawnFeatures, Is.EqualTo(expected));
    }

    [TestCase("\"minzoom\": 20")]
    [TestCase("\"maxzoom\": 5")]
    public void Render_LayerOutsideZoomRange_DrawsNothing(string zoomRange)
    {
        var canvas = new RecordingCanvas();

        VexTileRenderer.Render(CreateTile(), CreateStyle(zoomRange), canvas, 0, 0, 14);

        Assert.That(canvas.DrawnFeatures, Is.Empty);
    }

    private static VectorTile CreateTile()
    {
        var layer = new VectorTileLayer { Name = "transportation" };
        for (var i = 0; i < FeatureAttributes.Length; i++)
        {
            layer.Features.Add(new VectorTileFeature
            {
                GeometryType = "LineString",
                Attributes = new Dictionary<string, object>(FeatureAttributes[i]),
                Geometry = [[new Point(i, 0), new Point(i, 10)]],
            });
        }
        var tile = new VectorTile();
        tile.Layers.Add(layer);
        return tile;
    }

    private static VectorStyle CreateStyle(string layerProperties) => new(VectorStyleKind.Custom, customStyle: $$"""
        {
          "version": 8,
          "sources": { "openmaptiles": { "type": "vector" } },
          "layers": [
            {
              "id": "roads",
              "type": "line",
              "source": "openmaptiles",
              "source-layer": "transportation",
              {{layerProperties}},
              "paint": { "line-color": "#ff0000" }
            }
          ]
        }
        """);

    private sealed class RecordingCanvas : ICanvas
    {
        public List<int> DrawnFeatures { get; } = [];
        public bool ClipOverflow { get; set; }
        public SKColor BackgroundColor => SKColors.Transparent;
        public void DrawLineString(List<Point> geometry, Brush style) => DrawnFeatures.Add((int)geometry[0].X);
        public void DrawBackground(SKColor color) { }
        public void DrawPolygon(List<Point> geometry, Brush style, SKColor? background) { }
        public void DrawPoint(Point geometry, Brush style) { }
        public void DrawText(Point geometry, Brush style) { }
        public void DrawTextOnPath(List<Point> geometry, Brush style) { }
        public void DrawImage(byte[] imageData, Brush style) { }
        public void DrawUnknown(List<List<Point>> geometry, Brush style) { }
        public byte[] ToPngByteArray(int quality = 80) => [];
    }
}
