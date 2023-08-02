using Mapsui.Tiling;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Navigation;

internal class ZoomLockSample : ISample
{
    public string Name => "ZoomLock";
    public string Category => "Navigation";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Navigator.ZoomTo(4892);
        map.Navigator.ZoomLock = true;
        return Task.FromResult(map);
    }
}
