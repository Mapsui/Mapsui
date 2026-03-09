using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.SkiaWidgets;

public interface ISkiaWidgetRenderer : IWidgetRenderer
{
    /// <summary>
    /// Draws a widget onto the canvas.
    /// </summary>
    /// <param name="canvas">The Skia canvas to draw on. When <paramref name="dirtyScreenRect"/> is set
    /// the canvas is already clipped to that rectangle, so drawing outside it has no visible effect.</param>
    /// <param name="viewport">The current viewport.</param>
    /// <param name="widget">The widget to draw.</param>
    /// <param name="renderService">The render service, which holds shared caches and resources.</param>
    /// <param name="layerOpacity">Opacity to apply (0–1).</param>
    /// <param name="dirtyScreenRect">The screen-pixel rectangle currently being redrawn, or
    /// <see langword="null"/> when the full screen is being redrawn.
    /// Use this to skip expensive work when the widget falls entirely outside the refreshed area.</param>
    void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, Mapsui.Rendering.RenderService renderService, float layerOpacity, SKRect? dirtyScreenRect);
}
