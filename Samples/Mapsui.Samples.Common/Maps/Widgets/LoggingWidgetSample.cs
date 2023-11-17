using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.Samples.Common.PersistentCaches;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.PerformanceWidget;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class LoggingWidgetSample : IMapControlSample
{
    private IMapControl? _mapControl;

    public string Name => "LoggingWidget";

    public string Category => "Widgets";

    public void OnClick(object? sender, WidgetTouchedEventArgs args)
    {
        args.Handled = true;
    }

    public void Setup(IMapControl mapControl)
    {
        _mapControl = mapControl;

        //I like bing Hybrid
        mapControl.Map = BingSample.CreateMap(BingHybrid.DefaultCache, BruTile.Predefined.KnownTileSource.BingHybrid);

        var widget = new LoggingWidget(mapControl.Map, 7);

        // Add event handle, so that LoggingWidget gets all logs
        Logger.LogDelegate += widget.Log;

        widget.WidgetTouched += OnClick;

        mapControl.Map.Widgets.Add(widget);
        mapControl.Renderer.WidgetRenders[typeof(LoggingWidget)] = new LoggingWidgetRenderer(new MRect(10, 10, 210, 110), 12, Color.Black, Color.Orange, Color.Red, Color.White);

        Logger.Log(LogLevel.Trace, "Trace test", null);
        Logger.Log(LogLevel.Debug, "Debug test", null);
        Logger.Log(LogLevel.Information, "Information test with an extra long text", null);
        Logger.Log(LogLevel.Warning, "Warning test", null);
        Logger.Log(LogLevel.Error, "Error test", null);
    }
}
