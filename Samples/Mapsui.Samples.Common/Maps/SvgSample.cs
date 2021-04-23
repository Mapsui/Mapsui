using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class SvgSample : ISample
    {
        private static readonly ConcurrentDictionary<string, int> ImageCache = new ConcurrentDictionary<string, int>();
        public string Name => "Svg";
        public string Category => "Symbols";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateSvgLayer(map.Envelope));
            
            return map;
        }

        private static ILayer CreateSvgLayer(BoundingBox envelope)
        {
            return new MemoryLayer
            {
                Name = "Svg Layer",
                DataSource = CreateMemoryProviderWithDiverseSymbols(envelope, 2000),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        public static MemoryProvider<IGeometryFeature> CreateMemoryProviderWithDiverseSymbols(BoundingBox envelope, int count = 100)
        {
            return new MemoryProvider<IGeometryFeature>(CreateSvgFeatures(RandomPointHelper.GenerateRandomPoints(envelope, count)));
        }

        private static IEnumerable<IGeometryFeature> CreateSvgFeatures(IEnumerable<IGeometry> randomPoints)
        {
            var features = new List<IGeometryFeature>();
            var counter = 0;
            foreach (var point in randomPoints)
            {
                var feature = new Feature { Geometry = point, ["Label"] = counter.ToString() };
                feature.Styles.Add(CreateSvgStyle("Mapsui.Samples.Common.Images.Pin.svg", 0.5));
                features.Add(feature);
                counter++;
            }
            return features;
        }

        private static SymbolStyle CreateSvgStyle(string embeddedResourcePath, double scale)
        {
            var bitmapId = GetBitmapIdForEmbeddedResource(embeddedResourcePath);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = new Offset(0.0, 0.5, true) };
        }

        private static int GetBitmapIdForEmbeddedResource(string imagePath)
        {
            if (!ImageCache.TryGetValue(imagePath, out var id))
            {
                try
                {
                    var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
                    var image = assembly.GetManifestResourceStream(imagePath);
                    id = BitmapRegistry.Instance.Register(image);
                    ImageCache[imagePath] = id;
                }
                catch (Exception exception)
                {
                    Logger.Log(LogLevel.Error, $"Failed registering Image {imagePath}", exception);
                    throw;
                }
            }

            return id;
        }
    }
}