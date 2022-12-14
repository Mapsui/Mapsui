using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.Tiling;

namespace Mapsui.Samples.Common.Maps
{
    public class AtlasSample : ISample
    {
        private const string AtlasLayerName = "Atlas Layer";
        private static int _atlasBitmapId;
        private static readonly Random Random = new Random(1);

        public string Name => "Atlas";

        public string Category => "Styles";

        public Task<Map> CreateMapAsync()
        {
            _atlasBitmapId = typeof(AtlasSample).LoadBitmapId("Images.osm-liberty.png");
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateAtlasLayer(map.Extent));

            return Task.FromResult(map);
        }

        private static ILayer CreateAtlasLayer(MRect? envelope)
        {
            return new MemoryLayer
            {
                Name = AtlasLayerName,
                Features = CreateAtlasFeatures(RandomPointGenerator.GenerateRandomPoints(envelope, 1000)),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        private static IEnumerable<IFeature> CreateAtlasFeatures(IEnumerable<MPoint> randomPoints)
        {
            var counter = 0;

            return randomPoints.Select(p => {
                var feature = new PointFeature(p) { ["Label"] = counter.ToString() };

                var x = 0 + Random.Next(0, 12) * 21;
                var y = 64 + Random.Next(0, 6) * 21;
                var bitmapId = BitmapRegistry.Instance.Register(new Sprite(_atlasBitmapId, x, y, 21, 21, 1));
                feature.Styles.Add(new SymbolStyle { BitmapId = bitmapId });
                counter++;
                return feature;
            }).ToList();
        }
    }
}