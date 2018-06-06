using System;
using System.Collections.Generic;
using Mapsui.Rendering.Skia.Widgets;
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class WidgetRenderer
    {
        public static void Render(object target, double screenWidth, double screenHeight, IEnumerable<IWidget> widgets,
            IDictionary<Type, ISkiaWidgetRenderer> renders, float layerOpacity)
        {
            var canvas = (SKCanvas) target;

            foreach (var widget in widgets)
            {
                renders[widget.GetType()].Draw(canvas, screenWidth, screenHeight, widget, layerOpacity);
            }
        }
    }
}