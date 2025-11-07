using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Labels;

public class CustomPointStyleLabelSample : ISample
{
    public string Name => $"Label";
    public string Category => $"{nameof(CustomPointStyle)}";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new MemoryLayer($"{nameof(CustomPointStyle)}")
        {
            Features = CreateFeatures(map.Extent!, 32).ToList(),
            Style = new CustomPointStyle { RendererName = "custom-style-label" },
        });
        MapRenderer.RegisterPointStyleRenderer("custom-style-label", MyLabelCustomStyleRenderer);
        return map;
    }

    private static void MyLabelCustomStyleRenderer(SKCanvas canvas, IPointStyle style, RenderService renderService, float opacity)
    {
        using var paint = new SKPaint { Color = new SKColor(79, 10, 107, 192), IsAntialias = true };
        canvas.DrawCircle(0f, 0f, 10f, paint);
    }

    private static List<PointFeature> CreateFeatures(MRect envelope, int count) =>
        RandomPointsBuilder.GenerateRandomPoints(envelope, count, new Random(934))
            .Select(p => new PointFeature(p)).ToList();
}
