
using Mapsui.Widgets;
using Mapsui.Widgets.MousePositionWidget;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class MousePositionWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        TextBoxWidgetRenderer.DrawText(canvas, viewport, (MousePositionWidget)widget, layerOpacity);
    }
}
