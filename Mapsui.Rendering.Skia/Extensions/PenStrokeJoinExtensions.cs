using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class PenStrokeJoinExtensions
{
    public static SKStrokeJoin ToSkia(this StrokeJoin penStrokeJoin)
    {
        return penStrokeJoin switch
        {
            StrokeJoin.Miter => SKStrokeJoin.Miter,
            StrokeJoin.Round => SKStrokeJoin.Round,
            StrokeJoin.Bevel => SKStrokeJoin.Bevel,
            _ => SKStrokeJoin.Miter,
        };
    }
}
