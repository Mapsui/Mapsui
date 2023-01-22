using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

internal static class SKRectExtensions
{
    public static MRect ToMRect(this SKRect rect)
    {
        return new MRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
    }
}
