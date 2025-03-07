using Mapsui.Layers;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
public class CustomPointStyleBasicSample : ISample
{
    public string Name => $"{nameof(CustomPointStyle)}";
    public string Category => $"{nameof(CustomPointStyle)}";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new MemoryLayer($"{nameof(CustomPointStyle)}")
        {
            Features = CreateFeatures(map.Extent!, 32).ToList(),
            Style = new CustomPointStyle { RendererName = "custom-style-basic" },
        });
        MapRenderer.RegisterPointStyleRenderer("custom-style-basic", MyBasicCustomStyleRenderer);
        return map;
    }

    private static void MyBasicCustomStyleRenderer(SKCanvas canvas, IPointStyle style, RenderService renderService, float opacity)
    {
        using var paint = new SKPaint { Color = new SKColor(79, 10, 107, 192), IsAntialias = true };
        canvas.DrawCircle(0f, 0f, 10f, paint);
    }

    private static List<PointFeature> CreateFeatures(MRect envelope, int count) =>
        RandomPointsBuilder.GenerateRandomPoints(envelope, count, new Random(934))
            .Select(p => new PointFeature(p)).ToList();
}
