using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Samples.Common.Helpers
{
    public static class RandomPointHelper
    {
        private static Random _random = new Random(0);

        public static MemoryProvider CreateProviderWithRandomPoints(BoundingBox envelope, int count = 25, int seed = 123)
        {
            return new MemoryProvider(CreateFeatures(GenerateRandomPoints(envelope, count, seed)));
        }
        
        private static IEnumerable<IFeature> CreateFeatures(IEnumerable<IGeometry> randomPoints)
        {
            var counter = 0;
            return randomPoints.Select(p => new Feature { Geometry = p, ["Label"] = counter++.ToString() });
        }

        public static IEnumerable<IGeometry> GenerateRandomPoints(BoundingBox envelope, int count = 25, int seed = 192)
        {
            _random = new Random(seed);

            var result = new List<IGeometry>();

            for (var i = 0; i < count; i++)
            {
                result.Add(new Point(
                    _random.NextDouble() * envelope.Width + envelope.Left,
                    _random.NextDouble() * envelope.Height + envelope.Bottom));
            }

            return result;
        }
    }
}
