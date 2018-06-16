using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets
{
    public interface ISkiaWidgetRenderer : IWidgetRenderer
    {
        void Draw(SKCanvas canvas, double screenWidth, double screenHeight, IWidget widget,
            float layerOpacity);
    }
}
