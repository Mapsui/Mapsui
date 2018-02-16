using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class PointsSample
    {
        private static Random _random = new Random(0);

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateRandomPointLayer(map.Envelope));
            return map;
        }

        public static MemoryProvider CreateProviderWithRandomPoints(BoundingBox envelope, int count = 100)
        {
            return new MemoryProvider(CreateFeatures(GenerateRandomPoints(envelope, count)));
        }

        public static IEnumerable<IGeometry> GenerateRandomPoints(BoundingBox envelope, int count = 25, int? randomSeed = null)
        {
            if (randomSeed != null) _random = new Random(randomSeed.Value);

            var result = new List<IGeometry>();

            for (var i = 0; i < count; i++)
            {
                result.Add(new Point(
                    _random.NextDouble()*envelope.Width + envelope.Left,
                    _random.NextDouble()*envelope.Height + envelope.Bottom));
            }

            return result;
        }

        private static Features CreateFeatures(IEnumerable<IGeometry> randomPoints)
        {
            var features = new Features();
            var counter = 0;
            foreach (var point in randomPoints)
            {
                features.Add(new Feature { Geometry = point, ["Label"] = counter++.ToString() });
            }
            return features;
        }

        public static ILayer CreateRandomPointLayer(BoundingBox envelope, int count = 25, IStyle style = null)
        {
            var color = Color.White;
            color.A = 127;
            return new Layer
            {
                DataSource = new MemoryProvider(GenerateRandomPoints(envelope, count)),
                Style = style ?? new VectorStyle { Fill = new Brush(color) }
            };
        }
    }
}