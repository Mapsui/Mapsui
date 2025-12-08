using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Experimental.VectorTiles.Tiling;
using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Tiling.Utilities;
using Mapsui.Widgets.InfoWidgets;
using MarinerNotices.MapsuiBuilder.LayerBuilders;
using System.Linq;


//using MarinerNotices.MapsuiBuilder.LayerBuilders;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Tiles;

public sealed class VectorTilesSample : ISample
{
    public string Name => "VectorTiles";
    public string Category => "1";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateTileLayer());
        map.Widgets.Add(new MapInfoWidget(map, [map.Layers.Last()]));
        return map;
    }

    private static TileLayer CreateTileLayer()
    {
        var userAgent = HttpClientTools.GetDefaultApplicationUserAgent();
        var attribution = new BruTile.Attribution("© OpenStreetMap contributors", "https://www.openstreetmap.org/copyright");

        var httpTileSource = new HttpTileSource(
            new GlobalSphericalMercator(),
            "https://localhost:7272/api/VectorTile/GetTile?col={x}&row={y}&level={z}",
            //!!!"https://vector.openstreetmap.org/shortbread_v1/{z}/{x}/{y}.mvt",
            name: "Shortbread",
            attribution: attribution,
            configureHttpRequestMessage: (r) => r.Headers.TryAddWithoutValidation("User-Agent", userAgent));

        var featureTileSource = new FeatureHttpTileSource(httpTileSource);

        return new TileLayer(featureTileSource)
        {
            Name = "VectorTiles",
            //!!!Style = new VectorTileStyle(new VectorStyle { Fill = null }),
            Style = new VectorTileStyle(VectorTileStyleBuilder.CreateStyle()),
        };
    }
}
