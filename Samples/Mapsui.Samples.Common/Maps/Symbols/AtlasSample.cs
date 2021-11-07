using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Layers.Tiling;
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
            map.Layers.Add(CreateAtlasLayer(map.Extent));

            return map;
        }

        private static ILayer CreateAtlasLayer(MRect envelope)
        {
            return new MemoryLayer
            {
                Name = AtlasLayerName,
                DataSource = CreateMemoryProviderWithDiverseSymbols(envelope, 1000),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        public static GeometryMemoryProvider<IGeometryFeature> CreateMemoryProviderWithDiverseSymbols(MRect envelope, int count = 100)
        {
            var points = RandomPointGenerator.GenerateRandomPoints(envelope, count).Select(p => p.ToPoint());
            return new GeometryMemoryProvider<IGeometryFeature>(CreateAtlasFeatures(points));
        }

        private static IEnumerable<IGeometryFeature> CreateAtlasFeatures(IEnumerable<IGeometry> randomPoints)
        {
            var features = new List<IGeometryFeature>();
            var counter = 0;
            foreach (var point in randomPoints)
            {
                var feature = new GeometryFeature { Geometry = point, ["Label"] = counter.ToString() };

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