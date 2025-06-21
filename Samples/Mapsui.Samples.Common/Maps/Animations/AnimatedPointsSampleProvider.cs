// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Animations;

internal class AnimatedPointsSampleProvider : MemoryProvider, IDynamic, IDisposable
{
    private readonly Timer _timer;
    private readonly Random _random = new(0);
    private List<PointFeature> _previousFeatures = new();

    public AnimatedPointsSampleProvider()
    {
        _timer = new Timer(_ => DataHasChanged(), this, 0, 1600);
    }

    public event EventHandler? DataChanged;

    public override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var features = CreateNewFeatures(fetchInfo, _random, _previousFeatures);
        _previousFeatures = MergeWithPreviousFeatures(_previousFeatures, features);
        return Task.FromResult((IEnumerable<IFeature>)features);
    }

    private static List<PointFeature> CreateNewFeatures(FetchInfo fetchInfo, Random random, List<PointFeature> previousFeatures)
    {
        var features = new List<PointFeature>();
        var points = RandomPointsBuilder.GenerateRandomPoints(fetchInfo.Extent, 10, random).ToList();
        var count = 0;
        var randomItemToSkip = random.Next(points.Count);

        foreach (var point in points)
        {
            count++;
            if (count == randomItemToSkip)
                continue; // Skip a random element to test robustness.

            var countAsString = count.ToString(CultureInfo.InvariantCulture);
            features.Add(new PointFeature(point)
            {
                ["ID"] = countAsString,
                ["rotation"] = (AngleOf(point, FindPreviousPosition(countAsString, previousFeatures), random) - 90 + 360) % 360
            });
        }

        return features;
    }

    private static MPoint? FindPreviousPosition(string countAsString, List<PointFeature> previousFeatures)
    {
        return previousFeatures.FirstOrDefault(f => f["ID"]?.ToString() == countAsString)?.Point;
    }

    public static double AngleOf(MPoint point1, MPoint? point2, Random random)
    {
        if (point2 == null)
            return random.Next(360);
        double result = Algorithms.RadiansToDegrees(Math.Atan2(point1.Y - point2.Y, point2.X - point1.X));
        return (result < 0) ? (360.0 + result) : result;
    }

    public void DataHasChanged()
    {
        OnDataChanged();
    }

    private void OnDataChanged()
    {
        DataChanged?.Invoke(this, new EventArgs());
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
        }
    }
    private static List<PointFeature> MergeWithPreviousFeatures(List<PointFeature> previousFeatures, List<PointFeature> newFeatures)
    {
        // Some features are missing in the new list (to test robustness). We want to store the missing ones as well.
        return newFeatures
            .Concat(previousFeatures)
            .GroupBy(f => f["ID"])
            .Select(g => g.First())
            .ToList();
    }
}
