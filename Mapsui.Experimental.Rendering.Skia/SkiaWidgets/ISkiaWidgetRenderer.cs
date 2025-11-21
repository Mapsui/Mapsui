using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.SkiaWidgets;

public interface ISkiaWidgetRenderer : IWidgetRenderer
{
    void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, Mapsui.Rendering.RenderService renderService, float layerOpacity);
}
