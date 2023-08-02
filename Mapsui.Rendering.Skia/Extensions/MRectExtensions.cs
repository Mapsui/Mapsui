using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class MRectExtensions
{
    public static SKRect ToSkia(this MRect rect)
    {
        return new SKRect((float)rect.MinX, (float)rect.MinY, (float)rect.MaxX, (float)rect.MaxY);
    }
}
