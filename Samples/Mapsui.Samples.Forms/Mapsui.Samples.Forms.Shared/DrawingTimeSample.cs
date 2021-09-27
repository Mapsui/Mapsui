using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Forms;
using Mapsui.Widgets.DrawingTime;
using SkiaSharp;
using System;
using System.Linq;

namespace Mapsui.Samples.Forms.Shared
{
    public class DrawingTimeSample : IFormsSample
    {
        const int _maxValues = 100;
        double[] _values = new double[_maxValues];
        int _pos = 0;

        public string Name => "4 DrawingTimeWidget Sample";

        public string Category => "Widgets";

        public bool OnClick(object sender, EventArgs args)
        {
            return true;
        }

        public void Setup(IMapControl mapControl)
        {
            //I like bing Hybrid
            mapControl.Map = BingSample.CreateMap(BruTile.Predefined.KnownTileSource.BingHybrid);

            var widget = new DrawingTimeWidget();

            mapControl.Map.Widgets.Add(widget);
            mapControl.Renderer.WidgetRenders[typeof(DrawingTimeWidget)] = new DrawingTimeWidgetRenderer(10, 10, 12, SKColors.Black, SKColors.White);

            if (mapControl is MapView)
                mapControl = (MapControl)((MapView)mapControl).Content;

            ((MapControl)mapControl).PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(MapControl.DrawingTime))
                {
                    _values[_pos++] = ((MapControl)mapControl).DrawingTime;
                    if (_pos >= _maxValues)
                        _pos = 0;
                    widget.LastDrawingTime = _values.Average();
                }
            };
        }
    }
}
