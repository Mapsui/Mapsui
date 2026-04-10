using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Performance;

/// <summary>
/// This sample demonstrates the fastest point-rendering path: a CustomLayerRenderer inside a
/// RasterizingTileLayer. The RasterizingTileLayer keeps only tile bitmaps on screen at frame-rate;
/// the custom renderer is called once per tile (only when that tile is built or invalidated), where
/// it casts the layer to <see cref="PointDataLayer"/> and iterates a raw struct array — no IFeature
/// boxing, no GetFeatures call, no style or theme machinery in the hot path.
/// </summary>
public sealed class RasterizingTileLayerWithCustomLayerRendererSample : ISample
{
    public string Name => "RasterizingTileLayerWithCustomLayerRenderer";
    public string Category => "Performance";

    private const string RendererName = "rasterizing-tile-layer-custom-layer-renderer";

    static readonly SKPaint _paint = new() { Color = new SKColor(79, 10, 107, 192), IsAntialias = true };

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateRasterizingTileLayer());
        var extent = map.Layers.Get(1).Extent!.Grow(map.Layers.Get(1).Extent!.Width * 0.1);
        map.Navigator.ZoomToBox(extent);

        // Register with the standard Skia renderer.
        MapRenderer.RegisterLayerRenderer(RendererName, CustomLayerRenderer);
        // Also register for the experimental renderer (needed when running with LocalRendererConfig.cs).
        Experimental.Rendering.Skia.MapRenderer.RegisterLayerRenderer(RendererName, CustomLayerRenderer);

        return Task.FromResult(map);
    }

    private static RasterizingTileLayer CreateRasterizingTileLayer() => new(CreatePointDataLayer());

    private static void CustomLayerRenderer(SKCanvas canvas, Viewport viewport, ILayer layer, RenderService renderService)
    {
        var extent = viewport.ToExtent();
        if (extent is null) return;

        // Cast to the concrete type to access the raw struct array directly — no IFeature overhead.
        foreach (var p in ((PointDataLayer)layer).Data)
        {
            if (p.X < extent.MinX || p.X > extent.MaxX || p.Y < extent.MinY || p.Y > extent.MaxY) continue;
            var (screenX, screenY) = viewport.WorldToScreenXY(p.X, p.Y);
            canvas.DrawRect((float)screenX - 5f, (float)screenY - 5f, 10f, 10f, _paint);
        }
    }

    private static PointDataLayer CreatePointDataLayer()
    {
        var rnd = new Random(3462); // Fix the random seed so the points don't move after a refresh
        var data = new PointData[1_000_000];
        for (var i = 0; i < data.Length; i++)
            data[i] = new PointData(rnd.Next(-3_000_000, 3_000_000), rnd.Next(-3_000_000, 3_000_000));

        return new PointDataLayer("Points", data) { CustomLayerRendererName = RendererName };
    }
}

/// <summary>
/// A lightweight point value: x/y world coordinates and an optional name.
/// Using a struct avoids per-point heap allocations.
/// </summary>
public readonly record struct PointData(double X, double Y, string Name = "");

/// <summary>
/// A minimal layer that holds a plain <see cref="PointData"/> array.
/// Because <see cref="ILayer.CustomLayerRendererName"/> is set, Mapsui's
/// <c>VisibleFeatureIterator</c> never calls <see cref="GetFeatures"/>; the custom renderer
/// accesses <see cref="Data"/> directly by casting the <c>ILayer</c> to this type.
/// </summary>
public sealed class PointDataLayer(string name, PointData[] data) : BaseLayer(name)
{
    private readonly MRect? _dataExtent = ComputeExtent(data);

    public PointData[] Data { get; } = data;
    public override MRect? Extent => _dataExtent;

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution) => [];

    private static MRect? ComputeExtent(PointData[] points)
    {
        if (points.Length == 0) return null;
        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;
        foreach (var p in points)
        {
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }
        return new MRect(minX, minY, maxX, maxY);
    }
}
