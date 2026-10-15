using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.Tiling;
using SkiaSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

/// <summary>
/// Integrates an existing two-dimensional Skia drawing with the map by transforming the entire scene.
/// </summary>
public class CustomLayerMatrixSample : ISample
{
    private const string _rendererName = "custom-layer-matrix";
    private static readonly MPoint _origin = new(SphericalMercator.FromLonLat(4.9, 52.37));
    private static readonly SKPoint[] _nodes = [new(-120, -80), new(100, -80), new(100, 90), new(-120, 90)];

    public string Name => "Skia scene with a matrix";
    public string Category => nameof(CustomLayerRenderer);

    public Task<Map> CreateMapAsync()
    {
        var map = new Map { CRS = "EPSG:3857" };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        // A layer callback draws the existing scene in the same frame and viewport as the map.
        // The scene does not need to be converted into individual Mapsui features.
        map.Layers.Add(new MemoryLayer("Skia scene") { CustomLayerRendererName = _rendererName, Style = null });
        MapRenderer.RegisterLayerRenderer(_rendererName, DrawLayer);
        Experimental.Rendering.Skia.MapRenderer.RegisterLayerRenderer(_rendererName, DrawLayer);

        // The red Mapsui dots should stay at the centers of the orange Skia circles during pan, zoom and rotation.
        map.Layers.Add(new MemoryLayer("Alignment markers")
        {
            Features = _nodes.Select(p => new PointFeature(_origin.X + p.X, _origin.Y + p.Y)).ToArray(),
            Style = new SymbolStyle { Fill = new Brush(Color.Red), Outline = null, SymbolScale = 0.2 }
        });

        // Prepare the scene's paint before drawing; the map's cache owns its lifetime.
        using var paint = map.RenderService.VectorCache.GetOrCreate(_rendererName, CreatePaint);
        map.Navigator.CenterOnAndZoomTo(_origin, 1);
        map.Navigator.RotateTo(20);
        return Task.FromResult(map);
    }

    private static SKPaint CreatePaint(string _) => new() { Color = new SKColor(230, 100, 15), Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };

    private static void DrawLayer(SKCanvas canvas, Viewport viewport, ILayer layer, RenderService renderService)
    {
        var matrix = CreateSceneToScreenMatrix(viewport, _origin);
        using var paint = renderService.VectorCache.GetOrCreate(_rendererName, CreatePaint);

        // Optional: keep strokes two device-independent pixels wide while the geometry zooms.
        // Use a fixed width in scene units instead if the existing drawing's strokes should zoom too.
        var scenePaint = paint.Instance;
        scenePaint.StrokeWidth = (float)(2 * viewport.Resolution);

        using var restore = new SKAutoCanvasRestore(canvas);
        // Preserve the canvas's pixel-density scale. Replacing its matrix would lose that scale.
        canvas.Concat(matrix);
        DrawScene(canvas, scenePaint);
    }

    private static SKMatrix CreateSceneToScreenMatrix(Viewport viewport, MPoint origin)
    {
        // Scene coordinates are offsets in the map's projected CRS, with Y pointing north.
        // Project GNSS coordinates and georeference metric CAD data into that CRS first:
        // Web Mercator map units are not ground metres, and this matrix is not a geographic projection.
        // Subtract the nearby origin in double precision before storing scene coordinates as floats.
        var (screenX, screenY) = viewport.WorldToScreenXY(origin.X, origin.Y);
        var radians = viewport.IsRotated() ? Math.IEEERemainder(viewport.Rotation, 360) * Math.PI / 180 : 0;
        var cos = Math.Cos(radians) / viewport.Resolution;
        var sin = Math.Sin(radians) / viewport.Resolution;

        // Apply zoom, rotation and the north-up to screen-down Y flip to every scene primitive at once.
        return new SKMatrix((float)cos, (float)sin, (float)screenX,
            (float)sin, (float)-cos, (float)screenY, 0, 0, 1);
    }

    private static void DrawScene(SKCanvas canvas, SKPaint paint)
    {
        // This stands in for an existing Skia renderer: it knows nothing about Mapsui or its viewport.
        // Its coordinates must remain in scene space; do not also apply an independent camera transform.
        for (var i = 0; i < _nodes.Length; i++)
        {
            canvas.DrawLine(_nodes[i], _nodes[(i + 1) % _nodes.Length], paint);
            canvas.DrawCircle(_nodes[i], 12, paint);
        }
        canvas.DrawLine(_nodes[0], _nodes[2], paint);
        canvas.DrawRect(-35, -20, 70, 40, paint);
    }
}
