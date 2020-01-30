using System;
using System.Collections.Generic;
using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using SkiaSharp;

namespace Mapsui.Samples.Common.Maps
{
    public class DrawableSample : ISample
    {
        private const string DrawableLayerName = "Drawable Layer";
        private static readonly Random Random = new Random();

        public string Name => "Drawable";

        public string Category => "Symbols";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateDrawableLayer(map.Envelope));
           
            return map;
        }

        private static ILayer CreateDrawableLayer(BoundingBox envelope)
        {
            return new MemoryLayer
            {
                Name = DrawableLayerName,
                DataSource = CreateMemoryProviderWithDiverseSymbols(envelope, 10),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        public static MemoryProvider CreateMemoryProviderWithDiverseSymbols(BoundingBox envelope, int count = 100)
        {
            return new MemoryProvider(CreateDrawableFeatures(RandomPointHelper.GenerateRandomPoints(envelope, count)));
        }

        private static Features CreateDrawableFeatures(IEnumerable<IGeometry> randomPoints)
        {
            var features = new Features();
            var counter = 0;
            foreach (var point in randomPoints)
            {
                var feature = new Feature { Geometry = point, ["Label"] = counter.ToString() };

                feature.Styles.Add(new VectorStyle() { Line = new Pen(Color.Black) });

                var drawable = new Drawable();
                var bitmapId = BitmapRegistry.Instance.Register(drawable);
                feature.Styles.Add(new SymbolStyle { BitmapId = bitmapId });

                features.Add(feature);
                counter++;
            }
            return features;
        }
    }

    public class Drawable : SKDrawable
    {
        private static readonly Random Random = new Random();

        public Drawable() : base (true)
        { }

        protected override SKRect OnGetBounds()
        {
            return new SKRect(0, 0, 63, 63);
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            canvas.DrawRect(new SKRect(0, 0, 63, 63), new SKPaint() { Color = SKColors.Black, StrokeWidth = 1, Style = SKPaintStyle.Stroke });

            var paint = new SKPaint() { Color = new SKColor((byte)Random.Next(0,255), (byte)Random.Next(0, 255), (byte)Random.Next(0, 255)), StrokeWidth = 1 };
            var lastX = Random.Next(0, 63);
            var lastY = Random.Next(0, 63);
            for (var i = 0; i < 4; i++)
            {
                var x = Random.Next(0, 63);
                var y = Random.Next(0, 63);
                canvas.DrawLine(new SKPoint(lastX, lastY), new SKPoint(x, y), paint);
                lastX = x;
                lastY = y;
            }
        }

        protected override SKPicture OnSnapshot()
        {
            var rec = new SKPictureRecorder();
            var canvas = rec.BeginRecording(OnGetBounds());
            OnDraw(canvas);
            return rec.EndRecording();
        }
    }
}