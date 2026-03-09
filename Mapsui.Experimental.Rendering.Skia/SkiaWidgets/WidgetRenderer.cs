using System;
using System.Collections.Generic;
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.SkiaWidgets;

public static class WidgetRenderer
{
    /// <summary>
    /// Draws all enabled widgets onto the canvas.
    /// Widgets whose screen envelope does not overlap <paramref name="dirtyScreenRect"/> are skipped
    /// without calling their renderer (CPU-side early-out). On the first render a widget's envelope
    /// is not yet known, so it is always drawn so the envelope can be populated.
    /// </summary>
    /// <param name="target">The Skia canvas to draw on (cast to <c>SKCanvas</c> internally).</param>
    /// <param name="viewport">The current viewport.</param>
    /// <param name="widgets">The widgets to draw.</param>
    /// <param name="renders">Map of widget type to its renderer.</param>
    /// <param name="renderService">The render service, which holds shared caches and resources.</param>
    /// <param name="layerOpacity">Opacity to apply to all widgets (0–1).</param>
    /// <param name="dirtyScreenRect">The screen-pixel rectangle currently being redrawn, or
    /// <see langword="null"/> when the full screen is being redrawn.</param>
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
