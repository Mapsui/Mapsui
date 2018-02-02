using System.Collections.Generic;
using System.Windows.Controls;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;

namespace Mapsui.Rendering.Xaml
{
    public static class WidgetRenderer
    {
        public static void Render(object target, IEnumerable<IWidget> widgets)
        {
            var canvas = (Canvas)target;
            var widgetCanvas = new Canvas
            {
                Width = canvas.ActualWidth,
                Height = canvas.ActualHeight,
                Background = null
            };

            canvas.Children.Add(widgetCanvas);
            foreach (var widget in widgets)
            {
                if (widget is Hyperlink) HyperlinkWidgetRenderer.Draw(widgetCanvas, widget as Hyperlink);
                if (widget is ScaleBarWidget) ScaleBarWidgetRenderer.Draw(widgetCanvas, widget as ScaleBarWidget);
                if (widget is ZoomInOutWidget) ZoomInOutWidgetRenderer.Draw(widgetCanvas, widget as ZoomInOutWidget);
            }
        }
    }
}