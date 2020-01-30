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
    public class PictureSample : ISample
    {
        private const string PictureLayerName = "Picture Layer";
        private static readonly Random Random = new Random();

        public string Name => "Picture";

        public string Category => "Symbols";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreatePictureLayer(map.Envelope));
           
            return map;
        }

        private static ILayer CreatePictureLayer(BoundingBox envelope)
        {
            return new MemoryLayer
            {
                Name = PictureLayerName,
                DataSource = CreateMemoryProviderWithDiverseSymbols(envelope, 10),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        public static MemoryProvider CreateMemoryProviderWithDiverseSymbols(BoundingBox envelope, int count = 100)
        {
            return new MemoryProvider(CreatePictureFeatures(RandomPointHelper.GenerateRandomPoints(envelope, count)));
        }

        private static Features CreatePictureFeatures(IEnumerable<IGeometry> randomPoints)
        {
            var features = new Features();
            var counter = 0;
            foreach (var point in randomPoints)
            {
                var feature = new Feature { Geometry = point, ["Label"] = counter.ToString() };

                var rec = new SKPictureRecorder();
                var canvas = rec.BeginRecording(new SKRect(0, 0, 64, 64));
                var paint = new SKPaint() { Color = SKColors.Red, StrokeWidth = 1 };
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
                var pic = rec.EndRecording();
                var bitmapId = BitmapRegistry.Instance.Register(pic);
                feature.Styles.Add(new SymbolStyle { BitmapId = bitmapId });

                features.Add(feature);
                counter++;
            }
            return features;
        }
    }
}