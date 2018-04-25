using System.Collections.Generic;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class WidgetRenderer
    {
        public static void Render(object target, double screenWidth, double screenHeight, IEnumerable<IWidget> widgets,
            float layerOpacity)
        {
            var canvas = (SKCanvas)target;

            foreach (var widget in widgets)
            {
                if (widget is Hyperlink) HyperlinkWidgetRenderer.Draw(canvas, screenWidth, screenHeight, widget as Hyperlink, layerOpacity);
                if (widget is ScaleBarWidget) ScaleBarWidgetRenderer.Draw(canvas, screenWidth, screenHeight, widget as ScaleBarWidget, layerOpacity);
                if (widget is ZoomInOutWidget) ZoomInOutWidgetRenderer.Draw(canvas, screenWidth, screenHeight, widget as ZoomInOutWidget, layerOpacity);
            }
        }

    }
}