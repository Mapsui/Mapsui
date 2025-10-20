using Mapsui.Tiling;

namespace Mapsui.Samples.Wpf.ViewModels;

public class MainViewModel
{
    public Map Map { get; }

    public MainViewModel()
    {
        Map = new Map();

        var baseLayer = OpenStreetMap.CreateTileLayer();
        Map.Layers.Add(baseLayer);
    }
}
