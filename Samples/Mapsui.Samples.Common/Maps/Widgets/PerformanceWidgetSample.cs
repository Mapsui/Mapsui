using BruTile.Predefined;
using Mapsui.Samples.Common.PersistentCaches;
using Mapsui.Styles;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using Mapsui.Widgets.InfoWidgets;
using System.Linq;
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

        // The PerformanceWidget is created as part of the map.
        var performanceWidget = map.Widgets.OfType<PerformanceWidget>().First();
        performanceWidget.Performance.IsActive = Mapsui.Widgets.ActiveMode.Yes; // The default in ActiveMode.OnlyInDebugMode which is usually the best option. This is just to show how to change it.
        performanceWidget.BackColor = Color.FromRgba(255, 255, 32, 32);
        performanceWidget.Opacity = 1;

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
