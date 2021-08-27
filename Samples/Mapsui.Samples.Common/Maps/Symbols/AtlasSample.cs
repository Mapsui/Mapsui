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

namespace Mapsui.Samples.Common.Maps
{
    public class AtlasSample : ISample
    {
        private const string AtlasLayerName = "Atlas Layer";
        private static int _atlasBitmapId;
        private static readonly Random Random = new Random();

        public string Name => "Atlas";

        public string Category => "Symbols";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            _atlasBitmapId = BitmapRegistry.Instance.Register(typeof(AtlasSample).GetTypeInfo().Assembly.GetManifestResourceStream("Mapsui.Samples.Common.Images.osm-liberty.png"));

            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateAtlasLayer(map.Envelope));
           
            return map;
        }

        private static ILayer CreateAtlasLayer(BoundingBox envelope)
        {
            return new MemoryLayer
            {
                Name = AtlasLayerName,
                DataSource = CreateMemoryProviderWithDiverseSymbols(envelope, 1000),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        public static MemoryProvider CreateMemoryProviderWithDiverseSymbols(BoundingBox envelope, int count = 100)
        {
            return new MemoryProvider(CreateAtlasFeatures(RandomPointHelper.GenerateRandomPoints(envelope, count)));
        }

        private static Features CreateAtlasFeatures(IEnumerable<IGeometry> randomPoints)
        {
            var features = new Features();
            var counter = 0;
            foreach (var point in randomPoints)
            {
                var feature = new Feature { Geometry = point, ["Label"] = counter.ToString() };

                var x = 0 + Random.Next(0, 12) * 21;
                var y = 64 + Random.Next(0, 6) * 21;
                var bitmapId = BitmapRegistry.Instance.Register(new Sprite(_atlasBitmapId, x, y, 21, 21, 1));
                feature.Styles.Add(new SymbolStyle { BitmapId = bitmapId });

                features.Add(feature);
                counter++;
            }
            return features;
        }
    }
}