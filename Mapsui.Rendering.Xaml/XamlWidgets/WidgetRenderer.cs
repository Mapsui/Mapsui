using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Mapsui.Widgets;

namespace Mapsui.Rendering.Xaml.XamlWidgets
{
    public static class WidgetRenderer
    {
        public static void Render(object target, IReadOnlyViewport viewport, 
            IEnumerable<IWidget> widgets, IDictionary<Type, IWidgetRenderer> renderers)
        {
            var canvas = (Canvas)target;
            var widgetCanvas = new Canvas
            {
                Width = canvas.ActualWidth,
                Height = canvas.ActualHeight,
                Background = null
            };

            widgetCanvas.Arrange(new System.Windows.Rect(canvas.RenderSize));

            canvas.Children.Add(widgetCanvas);
            foreach (var widget in widgets)
            {
                ((IXamlWidgetRenderer)renderers[widget.GetType()]).Draw(widgetCanvas, viewport, widget);
            }
        }
    }
}