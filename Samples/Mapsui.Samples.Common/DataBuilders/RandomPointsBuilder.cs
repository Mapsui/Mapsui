using Mapsui.Layers;
using Mapsui.Providers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Samples.Common.DataBuilders;

public static class RandomPointsBuilder
{
    public static IEnumerable<PointFeature> CreateRandomFeatures(MRect? envelope, int count, Random? random = null)
    {
        random ??= new Random(123);

        return CreateFeatures(GenerateRandomPoints(envelope, count, random));
    }

    public static MemoryProvider CreateProviderWithRandomPoints(MRect? envelope, int count, Random? random = null)
    {
        random ??= new Random(123);

        return new MemoryProvider(CreateFeatures(GenerateRandomPoints(envelope, count, random)));
    }

    private static IEnumerable<PointFeature> CreateFeatures(IEnumerable<MPoint> randomPoints)
    {
        var counter = 0;
        return randomPoints.Select(p => new PointFeature(p) { ["Label"] = counter++.ToString() });
    }

    public static IEnumerable<MPoint> GenerateRandomPoints(MRect? envelope, int count, Random? random = null)
    {
        random ??= new Random(192);

        var result = new List<MPoint>();
        if (envelope == null)
            return result;

        for (var i = 0; i < count; i++)
        {
            result.Add(new MPoint(
                random.NextDouble() * envelope.Width + envelope.Left,
                random.NextDouble() * envelope.Height + envelope.Bottom));
        }

        return result;
    }
}
