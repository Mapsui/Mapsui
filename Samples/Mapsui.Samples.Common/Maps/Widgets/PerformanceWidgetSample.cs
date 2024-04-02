using Mapsui.Extensions;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.Samples.Common.PersistentCaches;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using System.Drawing;

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

        mapControl.Map = BingSample.CreateMap(BingHybrid.DefaultCache, BruTile.Predefined.KnownTileSource.BingHybrid);

        var widget = CreatePerformanceWidget();

        mapControl.Map.Widgets.Add(widget);
        mapControl.Performance = _performance;
        mapControl.Renderer.WidgetRenders[typeof(PerformanceWidget)] = new PerformanceWidgetRenderer();
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
