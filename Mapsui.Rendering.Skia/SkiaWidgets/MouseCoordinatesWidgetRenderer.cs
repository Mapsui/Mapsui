using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class MouseCoordinatesWidgetRenderer : TextBoxWidgetRenderer
{
    public override void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        base.Draw(canvas, viewport, (MouseCoordinatesWidget)widget, layerOpacity);
    }
}
