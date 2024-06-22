using Mapsui.Rendering.Skia.Cache;
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public interface ISkiaWidgetRenderer : IWidgetRenderer
{
    void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, RenderService renderService, float layerOpacity);
}
