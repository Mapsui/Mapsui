using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Widgets
{
    public interface ISkiaWidgetRenderer
    {
        void Draw(SKCanvas canvas, double screenWidth, double screenHeight, IWidget zoomInOut,
            float layerOpacity);
    }
}
