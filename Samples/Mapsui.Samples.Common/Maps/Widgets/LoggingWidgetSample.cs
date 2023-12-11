using System;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.Samples.Common.PersistentCaches;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.PerformanceWidget;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Projections;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class LoggingWidgetSample : ISample
{
    public string Name => "LoggingWidget";

    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        //I like bing Hybrid
        var map = BingSample.CreateMap(BingHybrid.DefaultCache, BruTile.Predefined.KnownTileSource.BingHybrid);

        var widget = new LoggingWidget(map, 14)
        {
            TextSize = 12,
            BackgroundColor = Color.White,
            Opacity = 0.8f,
            ErrorTextColor = Color.Red,
            WarningTextColor = Color.Orange,
            InformationTextColor = Color.Black,
            Envelope = new MRect(10, 10, 260, 210),
            Margin = 2
        };

        // Add event handle, so that LoggingWidget gets all logs
        Logger.LogDelegate += widget.Log;

        widget.WidgetTouched += OnClick;

        map.Widgets.Add(widget);

        map.Navigator.ViewportChanged += (s, e) =>
        {
            (var lon, var lat) = SphericalMercator.ToLonLat(map.Navigator.Viewport.CenterX, map.Navigator.Viewport.CenterY);

            Logger.Log(LogLevel.Information, $"Map center at {lat:0.000}/{lon:0.000}");
        };

        Logger.Log(LogLevel.Trace, "Trace test", null);
        Logger.Log(LogLevel.Debug, "Debug test", null);
        Logger.Log(LogLevel.Information, "Information test with an extra long text", null);
        Logger.Log(LogLevel.Warning, "Warning test", null);
        Logger.Log(LogLevel.Error, "Error test", null);

        return Task.FromResult(map);
    }

    public void OnClick(object? sender, WidgetTouchedEventArgs args)
    {
        args.Handled = true;
    }
}
