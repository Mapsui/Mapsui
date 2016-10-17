using System;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Samples.Common.Maps
{
    public static class RasterizingLayerSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OsmSample.CreateLayer());
            map.Layers.Add(new RasterizingLayer(CreateRandomPointLayer()));
            return map;
        }

        private static MemoryLayer CreateRandomPointLayer()
        {
            var provider = new MemoryProvider();
            var rnd = new Random();
            for (var i = 0; i < 100; i++)
            {
                var feature = new Feature
                {
                    Geometry = new Geometries.Point(rnd.Next(0, 5000000), rnd.Next(0, 5000000))
                };
                provider.Features.Add(feature);
            }
            var layer = new MemoryLayer {DataSource = provider};
            return layer;
        }
    }
}