using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Navigation;

public class KeepWithinExtentSample : ISample
{
    public string Name => "Keep Within Extent";
    public string Category => "Navigation";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Limiter = new ViewportLimiterKeepWithin
        {
            PanLimits = GetLimitsOfMadagaskar()
        };
        return Task.FromResult(map);
    }

    private static MRect GetLimitsOfMadagaskar()
    {
        var (minX, minY) = SphericalMercator.FromLonLat(41.8, -27.2);
        var (maxX, maxY) = SphericalMercator.FromLonLat(52.5, -11.6);
        return new MRect(minX, minY, maxX, maxY);
    }
}
