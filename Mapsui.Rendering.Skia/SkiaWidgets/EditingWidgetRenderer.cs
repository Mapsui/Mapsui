using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;
public class EditingWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        // use all of the canvas
        widget.Envelope = new MRect(0, 0, viewport.Width, viewport.Height);
    }
}
