
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class HyperlinkWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        TextBoxWidgetRenderer.DrawText(canvas, viewport, (Hyperlink)widget, layerOpacity);
    }
}
