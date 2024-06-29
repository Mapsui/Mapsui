using BruTile.Predefined;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling.Extensions;
using Mapsui.Tiling.Layers;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.MapBuilders;

public class MapBuilderSample : ISample
{
    readonly MPoint _sphericalMercatorCoordinate = SphericalMercator.FromLonLat(-81.2497, 42.9837).ToMPoint();

    public string Name => "MapBuilder";
    public string Category => "MapBuilders";

    public Task<Map> CreateMapAsync()
        => Task.FromResult(new MapBuilder()
            .WithMapConfiguration(map => map.Navigator.CenterOnAndZoomTo(_sphericalMercatorCoordinate, 1222.99)) // Navigation is complex, because the Map is passed as argument the navigation methods could be called. Better to have specific builder methods for navigation.
            .WithMapCRS("EPSG:3857") // Should we have such specific methods or should the configure method be enough?
            .WithMapConfiguration(map => map.CRS = "EPSG:3857") // Does the same thing as the line above.
            .WithLayer(() => new TileLayer(KnownTileSources.Create()))
            .WithZoomButtons()
            .WithScaleBarWidget(w => w.Margin = new MRect(16))
            .Build());
}
