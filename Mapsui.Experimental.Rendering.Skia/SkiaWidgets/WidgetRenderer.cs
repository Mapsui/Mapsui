using System;
using System.Collections.Generic;
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.SkiaWidgets;

public static class WidgetRenderer
{
    public static void Render(object target, Viewport viewport, IEnumerable<IWidget> widgets,
        IDictionary<Type, ISkiaWidgetRenderer> renders, Mapsui.Rendering.RenderService renderService, float layerOpacity,
        SKRect? dirtyScreenRect = null)
    {
        var canvas = (SKCanvas)target;

        foreach (var widget in widgets)
        {
            if (!widget.Enabled) continue;

            // If a dirty rect is active, skip widgets whose envelope is fully outside it.
            // Envelope is null on the first render — allow it through so Envelope gets populated.
            if (dirtyScreenRect is not null && widget.Envelope is not null)
            {
                var e = widget.Envelope;
                var d = dirtyScreenRect.Value;
                if (e.MaxX < d.Left || e.MinX > d.Right || e.MaxY < d.Top || e.MinY > d.Bottom)
                    continue;
            }

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

            renderer.Draw(canvas, viewport, widget, renderService, layerOpacity, dirtyScreenRect);
        }
    }
}
