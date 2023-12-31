using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

internal class MapInfoWidgetRenderer : TextBoxWidgetRenderer
{
    public override void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        base.Draw(canvas, viewport, widget, layerOpacity);
    }
}
