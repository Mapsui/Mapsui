using Mapsui.Tiling;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Navigation;

public class PanLockSample : IMapControlSample
{
    public string Name => "PanLock";
    public string Category => "Navigation";
    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
        // The MapControl that is part of this sample matters for the 
        // rendered result. When Map is assigned this triggers events that
        // initialize the extent. After that PanLock is set. If PanLock is set
        // before the assignment of the map it will be different. And also when
        // the entire map plus PanLock is created before it is assigned to the 
        // MapControl.Map. This is confusing. If you set PanLock it should be clear
        // what the extent is that you are locking to.
        mapControl.Map.PanLock = true;
    }

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        return map;
    }
}
