using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;
public static class SkPaintExtensions
{
    public static bool IsVisible(this SKPaint? paint)
    {
        return paint != null && paint.Color.Alpha != 0;
    }
}
