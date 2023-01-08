using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Navigation;

public class CenterOnLocationSample : ISample
{
    public string Name => "Center on location";

    public string Category => "Navigation";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        // Get the lon lat coordinates from somewhere (Mapsui can not help you there)
        var centerOfLondonOntario = new MPoint(-81.2497, 42.9837);
        // OSM uses spherical mercator coordinates. So transform the lon lat coordinates to spherical mercator
        var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfLondonOntario.X, centerOfLondonOntario.Y).ToMPoint();
        // Set the center of the viewport to the coordinate. The UI will refresh automatically
        // Additionally you might want to set the resolution, this could depend on your specific purpose
        map.Home = n => n.NavigateTo(sphericalMercatorCoordinate, map.Resolutions[9]);

        return Task.FromResult(map);
    }
}
