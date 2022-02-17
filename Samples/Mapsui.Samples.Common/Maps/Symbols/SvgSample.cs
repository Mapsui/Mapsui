using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.Tiling;
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
                feature.Styles.Add(CreateSvgStyle(@"Images.Pin.svg", 0.5));
                counter++;
                return feature;
            });
        }

        private static SymbolStyle CreateSvgStyle(string embeddedResourcePath, double scale)
        {
            var bitmapId = typeof(SvgSample).LoadSvgId(embeddedResourcePath);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = new Offset(0.0, 0.5, true) };
        }
    }
}