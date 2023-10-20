using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;
internal class MapInfoWidgetRenderer : TextBoxWidgetRenderer
{
    public override void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        base.Draw(canvas, viewport, widget, layerOpacity);

        // The MapInfoWidget should should listen to info event from all over the map
        widget.Envelope = new MRect(0, 0, viewport.Width, viewport.Height);        
    }
}
