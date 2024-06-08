using Mapsui.Rendering.Skia.Cache;
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;
public class InputOnlyWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, RenderService renderService, float layerOpacity)
    {
        // Do nothing. Widgets derived from the InputOnlyWidget class are not drawn.
    }
}
