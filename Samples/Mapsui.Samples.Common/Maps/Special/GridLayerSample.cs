using Mapsui.Layers;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Special;

public class GridLayerSample : ISample
{
    public string Name => "GridLayer";
    public string Category => "Special";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(new GridLayer { ShowCoordinateLabels = true });
        map.Layers.Add(CreatePointLayer());
        return map;
    }

    private static MemoryLayer CreatePointLayer()
    {
        var random = new Random(42);
        var features = new List<IFeature>();
        for (var i = 0; i < 20; i++)
            features.Add(new PointFeature(new MPoint(
                (random.NextDouble() - 0.5) * 20_000_000,
                (random.NextDouble() - 0.5) * 15_000_000)));

        return new MemoryLayer("Points")
        {
            Features = features,
            Style = new SymbolStyle(),
        };
    }
}
