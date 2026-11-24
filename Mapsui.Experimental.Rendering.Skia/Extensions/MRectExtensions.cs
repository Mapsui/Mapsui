using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Extensions;

public static class MRectExtensions
{
    public static SKRect ToSkia(this MRect rect)
    {
        return new SKRect((float)rect.MinX, (float)rect.MinY, (float)rect.MaxX, (float)rect.MaxY);
    }

    public static SKPath ToSkiaPath(this MRect rect)
    {
        using var builder = new SKPathBuilder();
        builder.AddRect(rect.ToSkia());
        return builder.Detach();
    }
}
