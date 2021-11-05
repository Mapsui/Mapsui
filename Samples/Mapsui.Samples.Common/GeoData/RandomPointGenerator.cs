﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Samples.Common.Helpers
{
    public static class RandomPointGenerator
    {
        private static Random _random = new Random(0);

        public static MemoryProvider<IPointFeature> CreateProviderWithRandomPoints(MRectangle envelope, int count = 25, int seed = 123)
        {
            return new MemoryProvider<IPointFeature>(CreateFeatures(GenerateRandomPoints(envelope, count, seed)));
        }

        private static IEnumerable<IPointFeature> CreateFeatures(IEnumerable<MPoint> randomPoints)
        {
            var counter = 0;
            return randomPoints.Select(p => new PointFeature { Point = p, ["Label"] = counter++.ToString() });
        }

        public static IEnumerable<MPoint> GenerateRandomPoints(MRectangle envelope, int count = 25, int seed = 192)
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
