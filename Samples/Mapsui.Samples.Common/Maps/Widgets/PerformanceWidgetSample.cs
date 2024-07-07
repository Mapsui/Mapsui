using BruTile.Predefined;
using Mapsui.Extensions;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Samples.Common.PersistentCaches;
using Mapsui.Styles;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class PerformanceWidgetSample : IMapControlSample
{
    private IMapControl? _mapControl;
    private readonly Mapsui.Utilities.Performance _performance = new(10);

    public string Name => "PerformanceWidget";

    public string Category => "Widgets";

    public void Setup(IMapControl mapControl)
    {
        _mapControl = mapControl;
        mapControl.Map = CreateMap();
        var widget = CreatePerformanceWidget();
        mapControl.Map.Widgets.Add(widget);
        mapControl.Performance = _performance;
        mapControl.Renderer.WidgetRenders[typeof(PerformanceWidget)] = new PerformanceWidgetRenderer();
    }


    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(CreateLayer());
        map.Navigator.CenterOnAndZoomTo(new MPoint(1059114.80157058, 5179580.75916194), map.Navigator.Resolutions[14]);
        map.BackColor = Color.FromString("#000613");

        return map;
    }

    private static TileLayer CreateLayer()
    {
        var apiKey = "Enter your api key here"; // Contact Microsoft about how to use this
        var tileSource = KnownTileSources.Create(KnownTileSource.BingHybrid, apiKey, BingHybrid.DefaultCache);
        return new TileLayer(tileSource, dataFetchStrategy: new DataFetchStrategy()) // DataFetchStrategy prefetches tiles from higher levels
        {
            Name = "Bing Aerial",
        };
    }

    private PerformanceWidget CreatePerformanceWidget() => new(_performance)
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
        Margin = new MRect(10),
        TextSize = 12,
        TextColor = Color.Black,
        BackColor = Color.White,
        Tapped = (s, e) =>
        {
            _mapControl?.Performance?.Clear();
            _mapControl?.RefreshGraphics();
            return true;
        }
    };
}
