﻿using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

public class CustomStyleSample : ISample
{
    private const string _mapInfoLayerName = "Custom Style Layer";

    public string Name => "Custom Style";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        // This is the crucial part where we tell the renderer that a CustomStyle should be
        // rendered with the SkiaCustomStyleRenderer
        MapRenderer.RegisterStyleRenderer(typeof(CustomStyle), new SkiaCustomStyleRenderer());

        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateStylesLayer(map.Extent));

        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == _mapInfoLayerName));

        return map;
    }

    private static MemoryLayer CreateStylesLayer(MRect? envelope) => new()
    {
        Name = _mapInfoLayerName,
        Features = CreateDiverseFeatures(RandomPointsBuilder.GenerateRandomPoints(envelope, 25)),
        Style = null,
    };

    private static List<IFeature> CreateDiverseFeatures(IEnumerable<MPoint> randomPoints)
    {
        var features = new List<IFeature>();
        var style = new CustomStyle();
        var counter = 1;
        foreach (var point in randomPoints)
        {
            var feature = new PointFeature(point);
            feature["Label"] = $"I'm no. {counter++} and, autsch, you hit me!";
            feature.Styles.Add(style); // Here the custom style is set!
            feature.Styles.Add(SmallDot());
            features.Add(feature);
        }
        return features;
    }

    private static SymbolStyle SmallDot() => new() { SymbolScale = 0.2, Fill = new Brush(new Color(40, 40, 40)) };

    public class CustomStyle : BaseStyle
    {
        public CustomStyle() => Opacity = 0.7f;
    }

    public class SkiaCustomStyleRenderer : ISkiaStyleRenderer
    {
        private static Random _random = new(1);

        public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService, long iteration)
        {
            if (feature is not PointFeature pointFeature) return false;
            var worldPoint = pointFeature.Point;

            var screenPoint = viewport.WorldToScreen(worldPoint);
            var color = new SKColor((byte)_random.Next(0, 256), (byte)_random.Next(0, 256), (byte)_random.Next(0, 256), (byte)(256.0 * layer.Opacity * style.Opacity));
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
}
