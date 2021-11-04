using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Providers;

namespace Mapsui.Samples.Common.Helpers
{
    public static class RandomPointGenerator
    {
        private static Random _random = new Random(0);

        public static GeometryMemoryProvider<IGeometryFeature> CreateProviderWithRandomPoints(MRect envelope, int count = 25, int seed = 123)
        {
            return new GeometryMemoryProvider<IGeometryFeature>(CreateFeatures(GenerateRandomPoints(envelope, count, seed)));
        }

        private static IEnumerable<IGeometryFeature> CreateFeatures(IEnumerable<MPoint> randomPoints)
        {
            var counter = 0;
            return randomPoints.Select(p => new GeometryFeature { Geometry = p.ToPoint(), ["Label"] = counter++.ToString() });
        }

        public static IEnumerable<MPoint> GenerateRandomPoints(MRect envelope, int count = 25, int seed = 192)
        {
            _random = new Random(seed);

            var result = new List<MPoint>();

            for (var i = 0; i < count; i++)
            {
                result.Add(new MPoint(
                    _random.NextDouble() * envelope.Width + envelope.Left,
                    _random.NextDouble() * envelope.Height + envelope.Bottom));
            }

            return result;
        }
    }
}
