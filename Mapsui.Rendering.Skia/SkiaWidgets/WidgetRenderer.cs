using System;
using System.Collections.Generic;
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public static class WidgetRenderer
{
    public static void Render(object target, Viewport viewport, IEnumerable<IWidget> widgets,
        IDictionary<Type, IWidgetRenderer> renders, float layerOpacity)
    {
        var canvas = (SKCanvas)target;

        foreach (var widget in widgets)
        {
            if (!widget.Enabled) continue;

            // Check if a renderer exists for this type of widget
            if (!renders.TryGetValue(widget.GetType(), out var renderer))
            {
                var type = widget.GetType();

                // Get BaseType of type until we there is no more BaseType or we found a renderer
                while (type != null && !renders.ContainsKey(type))
                    type = type.BaseType;

                // Did we find a renderer ...
                if (type == null)
                {
                    // ... no, so log an error and continue
                    Logging.Logger.Log(Logging.LogLevel.Error, $"Renderer for Widgets of type {widget.GetType()} not found");
                    continue;
                }

                // We found a BaseType with a renderer, so use this one
                renders[widget.GetType()] = renders[type];

                // Use this as renderer
                renderer = renders[widget.GetType()];
            }

            ((ISkiaWidgetRenderer)renderer).Draw(canvas, viewport, widget, layerOpacity);

            // Widget is redrawn
            widget.NeedsRedraw = false;
        }
    }
}
