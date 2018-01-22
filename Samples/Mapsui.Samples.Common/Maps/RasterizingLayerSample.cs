using System;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class RasterizingLayerSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(new RasterizingLayer(CreateRandomPointLayer()));
            return map;
        }

        private static MemoryLayer CreateRandomPointLayer()
        {
            var rnd = new Random();
            var features = new Features();
            for (var i = 0; i < 100; i++)
            {
                var feature = new Feature
                {
                    Geometry = new Geometries.Point(rnd.Next(0, 5000000), rnd.Next(0, 5000000))
                };
                features.Add(feature);
            }
            var provider = new MemoryProvider(features);

            var layer = new MemoryLayer {DataSource = provider};
            return layer;
        }
    }
}