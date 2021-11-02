﻿using System;
using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using SkiaSharp;

namespace Mapsui.Samples.Common.Maps
{
    public class CustomStyle : IStyle
    {
        public double MinVisible { get; set; } = 0;
        public double MaxVisible { get; set; } = double.MaxValue;
        public bool Enabled { get; set; } = true;
        public float Opacity { get; set; } = 0.7f;
    }

    public class SkiaCustomStyleRenderer : ISkiaStyleRenderer
    {
        public static Random Random = new();
        public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IGeometryFeature feature, IStyle style, ISymbolCache symbolCache)
        {
            if (!(feature.Geometry is Point worldPoint))
                return false;

            var screenPoint = viewport.WorldToScreen(worldPoint.ToMPoint());
            var color = new SKColor((byte)Random.Next(0, 256), (byte)Random.Next(0, 256), (byte)Random.Next(0, 256), (byte)(256.0 * layer.Opacity * style.Opacity));
            var colored = new SKPaint { Color = color, IsAntialias = true };
            var black = new SKPaint { Color = SKColors.Black, IsAntialias = true };

            canvas.Translate((float)screenPoint.X, (float)screenPoint.Y);
            canvas.DrawCircle(0, 0, 15, colored);
            canvas.DrawCircle(-8, -12, 8, colored);
            canvas.DrawCircle(8, -12, 8, colored);
            canvas.DrawCircle(8, -8, 2, black);
            canvas.DrawCircle(-8, -8, 2, black);

            using var path = new SKPath();
            path.ArcTo(new SKRect(-8, 2, 8, 10), 25, 135, true);
            canvas.DrawPath(path, new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Black, IsAntialias = true });

            return true;
        }
    }

    public class CustomStyleSample : ISample
    {
        public string Name => "Custom Style";
        public string Category => "Special";

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
            map.Layers.Add(CreateStylesLayer(map.Envelope));

            return map;
        }

        private static ILayer CreateStylesLayer(MRect envelope)
        {
            return new MemoryLayer
            {
                Name = "Custome Style Layer",
                DataSource = CreateMemoryProviderWithDiverseSymbols(envelope, 25),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        public static MemoryProvider<IGeometryFeature> CreateMemoryProviderWithDiverseSymbols(MRect envelope, int count = 100)
        {

            return new MemoryProvider<IGeometryFeature>(CreateDiverseFeatures(RandomPointHelper.GenerateRandomPoints(envelope, count)));
        }

        private static IEnumerable<IGeometryFeature> CreateDiverseFeatures(IEnumerable<MPoint> randomPoints)
        {
            var features = new List<IGeometryFeature>();
            var style = new CustomStyle();
            var counter = 1;
            foreach (var point in randomPoints)
            {
                var feature = new Feature { Geometry = point.ToPoint() };
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
}