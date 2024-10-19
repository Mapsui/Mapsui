using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Providers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Widgets.InfoWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Mapsui.Tiling;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Geometries;

public sealed class ManyMutatingLayersSample : ISample, IDisposable
{
    private bool _disposed;

    public string Name => "Many Mutating Layers";
    public string Category => "Performance";

    private readonly Random _random = new(123);
    private const int _featureCount = 40;
    private const int _layerCount = 20;
    private const double _c = 40075017 * 0.5; // Half the circumference of the earth
    private readonly Timer _timer1 = new(5);
    private readonly Timer _timer2 = new(6);
    private readonly Timer _timer3 = new(7);
    private readonly Timer _timer4 = new(8);
    private readonly Timer _timer5 = new(9);
    object _syncLock = new();

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        var features = RandomPointsBuilder.CreateRandomFeatures(new MRect(-_c, -_c, _c, _c), _featureCount, _random);
        map.Layers.Add(CreatePointLayers(_random, features).ToArray());
        map.Navigator.ZoomToBox(map.Layers.Get(0).Extent);

        map.Widgets.Add(new MapInfoWidget(map));

        InitializeTimer(_timer1, map, features);
        InitializeTimer(_timer2, map, features);
        InitializeTimer(_timer3, map, features);
        InitializeTimer(_timer4, map, features);
        InitializeTimer(_timer5, map, features);

        return Task.FromResult(map);
    }

    private double GetRandomNextDouble()
    {
        lock (_syncLock)
            return _random.NextDouble();
    }

    private void InitializeTimer(Timer timer, Map map, IEnumerable<PointFeature> features)
    {
        var random = new Random(38445);
        timer.Elapsed += (sender, args) => MutateFeatures(features, () => map.Refresh());
        timer.Start();
    }

    private void MutateFeatures(IEnumerable<PointFeature> features, Action refresh)
    {
        foreach (var feature in features)
        {
            feature.Point.X = feature.Point.X + (GetRandomNextDouble() - 0.5) * 4000;
            feature.Point.Y = feature.Point.Y + (GetRandomNextDouble() - 0.5) * 4000;
            feature.Modified();
            refresh();
        }
    }

    private static IEnumerable<Layer> CreatePointLayers(Random random, IEnumerable<IFeature> features)
    {
        for (var i = 0; i < _layerCount; i++)
        {
            yield return CreatePointLayer(i, features, random);
        }
    }

    private static Layer CreatePointLayer(int i, IEnumerable<IFeature> features, Random random)
    {
        return new Layer
        {
            Enabled = true,
            Name = $"Layer {i}",
            DataSource = CreateMemoryProvider(features),
            IsMapInfoLayer = true,
            Style = new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                SymbolScale = i * (1.0 / _layerCount) * 2.0,
                Outline = new Pen { Color = GenerateRandomColor(random), Width = 1 },
                Fill = null
            }
        };
    }

    private static MemoryProvider CreateMemoryProvider(IEnumerable<IFeature> features)
    {
        return new MemoryProvider(features);
    }

    public static Color GenerateRandomColor(Random random)
    {
        byte[] rgb = new byte[3];
        random.NextBytes(rgb);
        return new Color(rgb[0], rgb[1], rgb[2]);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _timer1.Dispose();
        _timer2.Dispose();
        _timer3.Dispose();
        _timer4.Dispose();
        _timer5.Dispose();

        _disposed = true;
    }
}
