using BruTile.Predefined;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using Mapsui.Widgets.InfoWidgets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
public class CustomPointStyleShaderSample : ISample
{
    public string Name => $"{nameof(CustomPointStyle)} Shader";
    public string Category => $"{nameof(CustomPointStyle)}";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(CreateLayer());
        map.Layers.Add(new MemoryLayer($"{nameof(CustomPointStyle)}")
        {
            Features = CreateFeatures(map.Extent!, 32).ToList(),
            Style = new CustomPointStyle() { RendererName = "custom-style-shader" },
        });
        map.Widgets.Add(new MapInfoWidget(map, [map.Layers.Last()]));

        MapRenderer.RegisterPointStyleRenderer("custom-style-shader", MyBasicCustomStyleRenderer);
        return map;
    }

    private static List<PointFeature> CreateFeatures(MRect envelope, int count) =>
        RandomPointsBuilder.GenerateRandomPoints(envelope, count, new Random(934))
        .Select(p => new PointFeature(p))
        .ToList();

    private static void MyBasicCustomStyleRenderer(SKCanvas canvas, IPointStyle style, RenderService renderService, float opacity)
    {
        using var paint = new SKPaint { Color = SKColors.OliveDrab, IsAntialias = true };
        canvas.DrawCircle(0f, 0f, 10f, paint);
        DrawEllipseWithGradient(canvas, CreatePath());
    }

    private static SKRect CreatePath()
    {
        var halfWidth = 20;
        var halfHeight = 20f;
        return new SKRect(-halfWidth, -halfHeight, halfWidth, halfHeight);
    }

    private static TileLayer CreateLayer()
    {
        var tileSource = KnownTileSources.Create(KnownTileSource.BKGTopPlusGrey);
        return new TileLayer(tileSource, dataFetchStrategy: new DataFetchStrategy()) // DataFetchStrategy prefetches tiles from higher levels
        {
            Name = "BKG Top Plus Grey",
        };
    }

    private static void DrawEllipseWithGradient(SKCanvas canvas, SKRect rect)
    {
        // create the shader
        var colors = new SKColor[] {
            new(0, 255, 255),
            new(255, 0, 255),
            new(255, 255, 0),
            new(0, 255, 255)
        };
        var shader = SKShader.CreateSweepGradient(new SKPoint(0, 0), colors, null);

        using var paint = new SKPaint { Shader = shader, IsAntialias = true };
        canvas.DrawOval(rect, paint);
    }
}
