
using Mapsui.Widgets;
using Mapsui.Widgets.MouseCoordinatesWidget;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class MouseCoordinatesWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        TextBoxWidgetRenderer.DrawText(canvas, viewport, (MouseCoordinatesWidget)widget, layerOpacity);
        // use all of the canvas
        widget.Envelope = new MRect(0, 0, viewport.Width, viewport.Height);
    }
}
