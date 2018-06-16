using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Mapsui.Widgets;

namespace Mapsui.Rendering.Xaml.XamlWidgets
{
    public static class WidgetRenderer
    {
        public static void Render(object target, IEnumerable<IWidget> widgets, IDictionary<Type, IWidgetRenderer> renders)
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
                ((IXamlWidgetRenderer)renders[widget.GetType()]).Draw(widgetCanvas, widget);
            }
        }
    }
}