using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Samples.Common.Helpers
{
    public static class RandomPointGenerator
    {
        private static Random _random = new Random(0);

        public static MemoryProvider<PointFeature> CreateProviderWithRandomPoints(MRect? envelope, int count = 25, int seed = 123)
        {
            return new MemoryProvider<PointFeature>(CreateFeatures(GenerateRandomPoints(envelope, count, seed)));
        }

        private static IEnumerable<PointFeature> CreateFeatures(IEnumerable<MPoint> randomPoints)
        {
            var counter = 0;
            return randomPoints.Select(p => new PointFeature(p) { ["Label"] = counter++.ToString() });
        }

        public static IEnumerable<MPoint> GenerateRandomPoints(MRect? envelope, int count = 25, int seed = 192)
        {
            _random = new Random(seed);

            var result = new List<MPoint>();
            if (envelope == null)
                return result;

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
