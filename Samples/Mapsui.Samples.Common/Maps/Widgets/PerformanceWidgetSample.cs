using BruTile.Predefined;
using Mapsui.Samples.Common.PersistentCaches;
using Mapsui.Styles;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class PerformanceWidgetSample : ISample
{
    public string Name => "PerformanceWidget";
    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(CreateBingTileLayer());
        map.Navigator.CenterOnAndZoomTo(new MPoint(1059114.80157058, 5179580.75916194), map.Navigator.Resolutions[14]);
        map.BackColor = Color.FromString("#000613");

        return map;
    }

    private static TileLayer CreateBingTileLayer()
    {
        var apiKey = "Enter your api key here"; // Contact Microsoft about how to use this
        var tileSource = KnownTileSources.Create(KnownTileSource.BingHybrid, apiKey, BingHybrid.DefaultCache);
        return new TileLayer(tileSource, dataFetchStrategy: new DataFetchStrategy()) // DataFetchStrategy prefetches tiles from higher levels
        {
            Name = "Bing Aerial",
        };
    }
}
