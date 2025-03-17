using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using SkiaSharp;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
public class CustomLayerRendererSample : ISample
{
    public string Name => $"{nameof(Rendering.Skia.CustomLayerRenderer)}";
    public string Category => $"{nameof(Rendering.Skia.CustomLayerRenderer)}";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer(map));
        MapRenderer.RegisterLayerRenderer("custom-layer-renderer", CustomLayerRenderer);
        map.Widgets.Add(new MapInfoWidget(map, map.Layers.OfType<MemoryLayer>));
        return map;
    }

    private static MemoryLayer CreatePointLayer(Map map)
    {
        return new MemoryLayer($"{nameof(CustomLayerRenderer)}")
        {
            Features = CreateFeatures(map.Extent!, 1_000).ToList(),
            Style = new SymbolStyle(),
            CustomLayerRendererName = "custom-layer-renderer"
        };
    }

    private static void CustomLayerRenderer(SKCanvas canvas, Viewport viewport, ILayer layer, RenderService renderService)
    {
        foreach (var feature in layer.GetFeatures(viewport.ToExtent(), viewport.Resolution))
        {
            var point = ((PointFeature)feature).Point;
            var style = new SymbolStyle { SymbolType = SymbolType.Rectangle };
            var opacity = (float)(layer.Opacity * style.Opacity);
            // Here the PointStyleRenderer is reused but you don't have to. You can also draw the point directly.
            PointStyleRenderer.DrawPointStyle(canvas, viewport, point.X, point.Y, style, renderService, opacity, DrawSymbolStyle);
        }
    }

    private static void DrawSymbolStyle(SKCanvas canvas, IPointStyle style, RenderService renderService, float opacity)
    {
        using var paint = new SKPaint { Color = new SKColor(79, 10, 107, 192), IsAntialias = true };
        canvas.DrawCircle(0f, 0f, 10f, paint);
    }

    private static PointFeature[] CreateFeatures(MRect envelope, int count) =>
        [.. RandomPointsBuilder.GenerateRandomPoints(envelope, count, new Random(934))
        .Select(p => new PointFeature(p))];
}
