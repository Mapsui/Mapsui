using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Navigation;

public class PanLockSample : ISample
{
    public string Name => "PanLock";
    public string Category => "Navigation";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        var saoPaulo = SphericalMercator.FromLonLat(-46.633, -23.55).ToMPoint();
        map.Navigator.CenterOnAndZoomTo(saoPaulo, 4892f);
        map.Navigator.PanLock = true;
        return map;
    }
}
