using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.UI;
using Mapsui.Utilities;
using Mapsui.Widgets;
using SkiaSharp;
using Mapsui.Extensions;
using Mapsui.Widgets.PerformanceWidget;

namespace Mapsui.Samples.Common.Maps
{
    public class PerformanceWidgetSample : ISample
    {
        IMapControl _mapControl;
        readonly Performance _performance = new Performance(10);

        public string Name => "4 Performance Widget";

        public string Category => "Widgets";

        public void OnClick(object sender, WidgetTouchedEventArgs args)
        {
            _mapControl?.Performance.Clear();
            _mapControl?.RefreshGraphics();

            args.Handled = true;
        }

        public void Setup(IMapControl mapControl)
        {
            _mapControl = mapControl;

            //I like bing Hybrid
            mapControl.Map = BingSample.CreateMap(BruTile.Predefined.KnownTileSource.BingHybrid);

            var widget = new PerformanceWidget(_performance);

            widget.WidgetTouched += OnClick;

            mapControl.Map.Widgets.Add(widget);
            mapControl.Performance = _performance;
            mapControl.Renderer.WidgetRenders[typeof(PerformanceWidget)] = new PerformanceWidgetRenderer(10, 10, 12, SKColors.Black, SKColors.White);
        }
    }
}
