﻿using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Special;

public sealed class MutatingTriangleSample : ISample, ISampleTest, IDisposable
{
    private static readonly Random _random = new(0);
    private static CancellationTokenSource? _cancelationTokenSource;

    public string Name => "Mutating triangle";
    public string Category => "Special";

    public Task<Map> CreateMapAsync()
    {
        _cancelationTokenSource?.Dispose();
        _cancelationTokenSource = new CancellationTokenSource();
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateMutatingTriangleLayer(map.Extent));
        return Task.FromResult(map);
    }

    private static MemoryLayer CreateMutatingTriangleLayer(MRect? envelope)
    {
        var layer = new MemoryLayer();

        var polygon = new Polygon(new LinearRing(GenerateRandomPoints(envelope, 3).ToArray()));
        var feature = new GeometryFeature(polygon);
        layer.Features = new List<IFeature> { feature };

        _ = PeriodicTask.RunAsync(() =>
        {
            feature.Geometry = new Polygon(new LinearRing(GenerateRandomPoints(envelope, 3).ToArray()));
            // Clear cache for change to show
            feature.Modified();
            // Trigger DataChanged notification
            layer.DataHasChanged();
        },
        TimeSpan.FromMilliseconds(1000));

        return layer;
    }

    public static IEnumerable<Coordinate> GenerateRandomPoints(MRect? envelope, int count = 25)
    {
        var result = new List<Coordinate>();
        if (envelope == null)
            return result;

        for (var i = 0; i < count; i++)
        {
            result.Add(new Coordinate(
                _random.NextDouble() * envelope.Width + envelope.Left,
                _random.NextDouble() * envelope.Height + envelope.Bottom));
        }

        result.Add(result[0].Copy()); // close polygon by adding start point.

        return result;
    }

    public class PeriodicTask
    {
        public static async Task RunAsync(Action action, TimeSpan period, CancellationToken? cancellationToken)
        {
            while (!(cancellationToken?.IsCancellationRequested ?? false))
            {
                if (cancellationToken == null)
                {
                    await Task.Delay(period);
                }
                else
                {
                    await Task.Delay(period, cancellationToken.Value);
                }

                if (!(cancellationToken?.IsCancellationRequested ?? false))
                    action();
            }
        }

        public static Task RunAsync(Action action, TimeSpan period)
        {
            return RunAsync(action, period, _cancelationTokenSource?.Token);
        }
    }

    public async Task InitializeTestAsync(IMapControl mapControl)
    {
        var localCancelationTokenSource = _cancelationTokenSource;
        if (localCancelationTokenSource is null) return;
        await localCancelationTokenSource.CancelAsync();
    }

    public void Dispose()
    {
        _cancelationTokenSource?.Cancel();
        _cancelationTokenSource?.Dispose();
    }
}
