using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.SkiaWidgets;

public class InputOnlyWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, Mapsui.Rendering.RenderService renderService, float layerOpacity)
    {
        // Do nothing. Widgets derived from the InputOnlyWidget class are not drawn.
    }
}
