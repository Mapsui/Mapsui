using Mapsui.Limiting;
using Mapsui.Tiling;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Navigation;

internal class ZoomLockSample : ISample
{
    public string Name => "ZoomLock";
    public string Category => "Navigation";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map { Viewport = { Limiter = { ZoomLock = true } } };
        map.Viewport.SetViewportStateWithLimit(map.Viewport.State with { Resolution = 4892 });
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        return Task.FromResult(map);
    }
}
