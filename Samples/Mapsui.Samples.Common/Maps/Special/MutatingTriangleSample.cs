using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Special;

public sealed class MutatingTriangleSample : ISample, ISampleTest, IDisposable
{
    public string Name => "Mutating triangle";
    public string Category => "Special";

    private static readonly Random Random = new(0);
    private static CancellationTokenSource? _cancelationTokenSource;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don\'t dispose injected")]
    public Task<Map> CreateMapAsync()
    {
        _cancelationTokenSource?.Dispose();
        _cancelationTokenSource = new CancellationTokenSource();
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateMutatingTriangleLayer(map.Extent));
        return Task.FromResult(map);
    }

    private static ILayer CreateMutatingTriangleLayer(MRect? envelope)
    {
        var layer = new MemoryLayer();

        var polygon = new Polygon(new LinearRing(GenerateRandomPoints(envelope, 3).ToArray()));
        var feature = new GeometryFeature(polygon);
        layer.Features = new List<IFeature> { feature };

        _ = PeriodicTask.RunAsync(() =>
        {
            feature.Geometry = new Polygon(new LinearRing(GenerateRandomPoints(envelope, 3).ToArray()));
            // Clear cache for change to show
            feature.RenderedGeometry.Clear();
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
                Random.NextDouble() * envelope.Width + envelope.Left,
                Random.NextDouble() * envelope.Height + envelope.Bottom));
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

    public Task InitializeTestAsync(IMapControl mapControl)
    {
        _cancelationTokenSource?.Cancel();
        return Task.CompletedTask;
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don\'t dispose injected")]
    public void Dispose()
    {
        _cancelationTokenSource?.Cancel();
        _cancelationTokenSource?.Dispose();
    }
}
