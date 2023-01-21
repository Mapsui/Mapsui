using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public interface ISkiaWidgetRenderer : IWidgetRenderer
{
    void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IWidget widget, float layerOpacity);
}
