using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mapsui.Layers;
using Mapsui.Layers.Tiling;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.UI;

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
            map.Layers.Add(CreateSvgLayer(map.Extent));

            return map;
        }

        private static ILayer CreateSvgLayer(MRect? envelope)
        {
            return new MemoryLayer
            {
                Name = "Svg Layer",
                DataSource = CreateMemoryProviderWithDiverseSymbols(envelope, 2000),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        public static MemoryProvider<IFeature> CreateMemoryProviderWithDiverseSymbols(MRect? envelope, int count = 100)
        {
            return new MemoryProvider<IFeature>(CreateSvgFeatures(RandomPointGenerator.GenerateRandomPoints(envelope, count)));
        }

        private static IEnumerable<IFeature> CreateSvgFeatures(IEnumerable<MPoint> randomPoints)
        {
            var counter = 0;

            return randomPoints.Select(p => {
                var feature = new PointFeature(p) { ["Label"] = counter.ToString() };
                feature.Styles.Add(CreateSvgStyle("Mapsui.Samples.Common.Images.Pin.svg", 0.5));
                counter++;
                return feature;
            });
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