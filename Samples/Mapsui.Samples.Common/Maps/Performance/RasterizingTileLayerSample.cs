using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Performance;

public class RasterizingTileLayerSample : ISample
{
    public string Name => "Rasterizing Tile Layer";
    public string Category => "Performance";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new RasterizingTileLayer(CreateRandomPointLayer()));
        var extent = map.Layers[1].Extent!.Grow(map.Layers[1].Extent!.Width * 0.1);
        map.Home = n => n.NavigateTo(extent);
        return Task.FromResult(map);
    }

    private static MemoryLayer CreateRandomPointLayer()
    {
        var rnd = new Random(3462); // Fix the random seed so the features don't move after a refresh
        var features = new List<IFeature>();
        for (var i = 0; i < 100; i++)
        {
            features.Add(new PointFeature(new MPoint(rnd.Next(0, 5000000), rnd.Next(0, 5000000))));
        }

        return new MemoryLayer
        {
            Features = features,
            Style = new SymbolStyle
            {
                SymbolType = SymbolType.Triangle,
                Fill = new Brush(Color.Red)
            }
        };
    }
}
