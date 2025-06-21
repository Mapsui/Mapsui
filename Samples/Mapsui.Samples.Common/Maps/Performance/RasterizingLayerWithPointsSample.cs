using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI;
using System;
using System.Collections.Generic;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Performance;

public class RasterizingLayerWithPointsSample : IMapControlSample
{
    public string Name => "RasterizingLayer with Points";
    public string Category => "Performance";

    public void Setup(IMapControl mapControl)
    {
        // PixelDensity is not always known at startup. The RasterizingLayer should be initialized later.
        var pixelDensity = mapControl.GetPixelDensity() ?? 1;
        mapControl.Map = CreateMap(pixelDensity);
    }

#pragma warning disable IDISP006 // Implement IDisposable
    static Layer? _layer;

    public static Map CreateMap(float pixelDensity)
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
#pragma warning disable IDISP003 // Dispose previous before re-assigning
        _layer = CreateRandomPointLayer();
#pragma warning restore IDISP003 // Dispose previous before re-assigning
        map.Layers.Add(new RasterizingLayer(_layer, pixelDensity: pixelDensity));
        var extent = map.Layers.Get(1).Extent!.Grow(map.Layers.Get(1).Extent!.Width * 0.1);
        map.Navigator.ZoomToBox(extent);
        map.Tapped += Map_Tapped;
        return map;
    }

    private static void Map_Tapped(object? sender, MapEventArgs e)
    {
        if (e.GestureType == Manipulations.GestureType.DoubleTap)
        {
            _layer!.DataSource = new MemoryProvider(CreateFeatures(_rnd));
            _layer.DataHasChanged();
        }
    }

    static Random _rnd = new Random(3462);
    private static Layer CreateRandomPointLayer()
    {
        var features = CreateFeatures(_rnd);

        return new Layer
        {
            Name = "Points",
            DataSource = new MemoryProvider(features),
            Style = new SymbolStyle
            {
                SymbolType = SymbolType.Triangle,
                Outline = new Pen(Color.Gray, 1f),
                Fill = new Brush(Color.Red)
            }
        };
    }

    private static List<IFeature> CreateFeatures(Random rnd)
    {
        var features = new List<IFeature>();
        for (var i = 0; i < 100; i++)
        {
            features.Add(new PointFeature(new MPoint(rnd.Next(0, 5000000), rnd.Next(0, 5000000))));
        }

        return features;
    }
}
