using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class PenStrokeCapExtensions
{
    public static SKStrokeCap ToSkia(this PenStrokeCap penStrokeCap)
    {
        return penStrokeCap switch
        {
            PenStrokeCap.Butt => SKStrokeCap.Butt,
            PenStrokeCap.Round => SKStrokeCap.Round,
            PenStrokeCap.Square => SKStrokeCap.Square,
            _ => SKStrokeCap.Butt,
        };
    }
}
