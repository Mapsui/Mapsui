using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class LoggingWidgetSample : ISample
{
    public string Name => "LoggingWidget";

    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map
        {
            CRS = "EPSG:3857"
        };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        var widget = new LoggingWidget()
        {
            LogLevelFilter = LogLevel.Trace,
            TextSize = 11,
            BackColor = Color.White,
            Opacity = 0.8f,
            ErrorTextColor = Color.Red,
            WarningTextColor = Color.Orange,
            InformationTextColor = Color.Black,
            Margin = new MRect(10, 20),
            Padding = new MRect(2),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

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

        widget.LogLevelFilter = LogLevel.Information;

        return Task.FromResult(map);
    }
}
