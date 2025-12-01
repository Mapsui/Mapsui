using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Extensions;

public static class SkPaintExtensions
{
    public static bool IsVisible(this SKPaint? paint)
    {
        return paint != null && paint.Color.Alpha != 0;
    }
}
