// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using Mapsui.Fetcher;
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
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once NotAccessedField.Local
    private readonly Timer _timer;
    private readonly Random _random = new(0);
    private IEnumerable<PointFeature> _previousFeatures = new List<PointFeature>();

    public AnimatedPointsSampleProvider()
    {
        _timer = new Timer(_ => DataHasChanged(), this, 0, 2000);
    }

    public event DataChangedEventHandler? DataChanged;

    public override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var features = new List<PointFeature>();
        var points = RandomPointsBuilder.GenerateRandomPoints(fetchInfo.Extent, 10, _random).ToList();
        var count = 0;
        var random = _random.Next(points.Count);

        foreach (var point in points)
        {
            count++;
            if (count == random) continue; // skip a random element to test robustness

            var countAsString = count.ToString(CultureInfo.InvariantCulture);
            features.Add(new PointFeature(point)
            {
                ["ID"] = countAsString,
                ["rotation"] = AngleOf(point, FindPreviousPosition(countAsString)) - 90
            });
        }

        _previousFeatures = features;
        return Task.FromResult((IEnumerable<IFeature>)features);
    }

    private MPoint? FindPreviousPosition(string countAsString)
    {
        return _previousFeatures.FirstOrDefault(f => f["ID"]?.ToString() == countAsString)?.Point;
    }

    public static double AngleOf(MPoint point1, MPoint? point2)
    {
        if (point2 == null) return 0;
        double result = Algorithms.RadiansToDegrees(Math.Atan2(point1.Y - point2.Y, point2.X - point1.X));
        return (result < 0) ? (360.0 + result) : result;
    }

    public void DataHasChanged()
    {
        OnDataChanged();
    }

    private void OnDataChanged()
    {
        DataChanged?.Invoke(this, new DataChangedEventArgs(null, false, null));
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
}
