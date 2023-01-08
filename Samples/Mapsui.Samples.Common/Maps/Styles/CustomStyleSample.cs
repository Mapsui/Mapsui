using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Mapsui.Samples.Common.Maps.Styles;

public class CustomStyle : IStyle
{
    public double MinVisible { get; set; } = 0;
    public double MaxVisible { get; set; } = double.MaxValue;
    public bool Enabled { get; set; } = true;
    public float Opacity { get; set; } = 0.7f;
}

public class SkiaCustomStyleRenderer : ISkiaStyleRenderer
{
    public static Random Random = new(1);
    public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, IRenderCache renderCache, long iteration)
    {
        if (feature is not PointFeature pointFeature) return false;
        var worldPoint = pointFeature.Point;

        var screenPoint = viewport.WorldToScreen(worldPoint);
        var color = new SKColor((byte)Random.Next(0, 256), (byte)Random.Next(0, 256), (byte)Random.Next(0, 256), (byte)(256.0 * layer.Opacity * style.Opacity));
        using var colored = new SKPaint { Color = color, IsAntialias = true };
        using var black = new SKPaint { Color = SKColors.Black, IsAntialias = true };

        canvas.Translate((float)screenPoint.X, (float)screenPoint.Y);
        canvas.DrawCircle(0, 0, 15, colored);
        canvas.DrawCircle(-8, -12, 8, colored);
        canvas.DrawCircle(8, -12, 8, colored);
        canvas.DrawCircle(8, -8, 2, black);
        canvas.DrawCircle(-8, -8, 2, black);

        using var path = new SKPath();
        path.ArcTo(new SKRect(-8, 2, 8, 10), 25, 135, true);
        using var skPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Black, IsAntialias = true };
        canvas.DrawPath(path, skPaint);

        return true;
    }
}

public class CustomStyleSample : IMapControlSample
{
    public string Name => "Custom Style";
    public string Category => "Styles";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();

        if (mapControl.Renderer is Rendering.Skia.MapRenderer && !mapControl.Renderer.StyleRenderers.ContainsKey(typeof(CustomStyle)))
            mapControl.Renderer.StyleRenderers.Add(typeof(CustomStyle), new SkiaCustomStyleRenderer());
    }

    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateStylesLayer(map.Extent));

        return map;
    }

    private static ILayer CreateStylesLayer(MRect? envelope)
    {
        return new MemoryLayer
        {
            Name = "Custome Style Layer",
            Features = CreateDiverseFeatures(RandomPointsBuilder.GenerateRandomPoints(envelope, 25)),
            Style = null,
            IsMapInfoLayer = true
        };
    }

    private static IEnumerable<IFeature> CreateDiverseFeatures(IEnumerable<MPoint> randomPoints)
    {
        var features = new List<IFeature>();
        var style = new CustomStyle();
        var counter = 1;
        foreach (var point in randomPoints)
        {
            var feature = new PointFeature(point);
            feature["Label"] = $"I'm no. {counter++} and, \nautsch, you hit me!";
            feature.Styles.Add(style); // Here the custom style is set!
            feature.Styles.Add(SmalleDot());
            features.Add(feature);
        }
        return features;
    }

    private static IStyle SmalleDot()
    {
        return new SymbolStyle { SymbolScale = 0.2, Fill = new Brush(new Color(40, 40, 40)) };
    }
}
