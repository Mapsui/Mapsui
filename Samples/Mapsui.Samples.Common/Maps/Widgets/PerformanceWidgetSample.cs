using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.UI;
using Mapsui.Utilities;
using Mapsui.Widgets.Performance;
using SkiaSharp;
using System;

namespace Mapsui.Samples.Common.Maps
{
    public class PerformanceWidgetSample : ISample
    {
        Performance _performance = new Performance(100);

        public string Name => "4 PerformanceWidget Sample";

        public string Category => "Widgets";

        public bool OnClick(object sender, EventArgs args)
        {
            return true;
        }

        public void Setup(IMapControl mapControl)
        {
            //I like bing Hybrid
            mapControl.Map = BingSample.CreateMap(BruTile.Predefined.KnownTileSource.BingHybrid);

            var widget = new PerformanceWidget(_performance);

            mapControl.Map.Widgets.Add(widget);
            mapControl.Performance = _performance;
            mapControl.Renderer.WidgetRenders[typeof(PerformanceWidget)] = new PerformanceWidgetRenderer(10, 10, 12, SKColors.Black, SKColors.White);
        }
    }
}
