using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Mapsui.Tiling.Layers;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Performance;

public class RasterizingTileLayerWithDynamicPointsSample : IMapControlSample
{
    public string Name => "RasterizingTileLayer with Dynamic Points";
    public string Category => "Performance";

    public void Setup(IMapControl mapControl)
    {
        // PixelDensity is not always known at startup. The RasterizingTileLayer should be initialized later.
        var pixelDensity = mapControl.GetPixelDensity() ?? 1;
        mapControl.Map = CreateMap(pixelDensity);
    }

    public static Map CreateMap(float pixelDensity)
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new RasterizingTileLayer(CreateRandomPointLayer(), pixelDensity: pixelDensity));
        var extent = map.Layers.Get(1).Extent!.Grow(map.Layers.Get(1).Extent!.Width * 0.1);
        map.Navigator.ZoomToBox(extent);
        return map;
    }

    private static ObservableMemoryLayer<MPoint> CreateRandomPointLayer()
    {
        var rnd = new Random(3462); // Fix the random seed so the features don't move after a refresh
        var observableCollection = new ObservableCollection<MPoint>();

        var layer = new ObservableMemoryLayer<MPoint>(f => new PointFeature(f))
        {
            Name = "Points",
            Style = new SymbolStyle
            {
                SymbolType = SymbolType.Triangle,
                Fill = new Brush(Color.Red)
            },
            ObservableCollection = observableCollection,
        };

        _ = Task.Run(async () =>
        {
            for (var i = 0; i < 100; i++)
            {
                observableCollection.Add(new MPoint(rnd.Next(0, 5000000), rnd.Next(0, 5000000)));
                layer.DataHasChanged();
                await Task.Delay(100);
            }
        });

        return layer;
    }
}
