using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class PenStrokeJoinExtensions
    {
        public static SKStrokeJoin ToSkia(this StrokeJoin penStrokeJoin)
        {
            switch (penStrokeJoin)
            {
                case StrokeJoin.Miter:
                    return SKStrokeJoin.Miter;
                case StrokeJoin.Round:
                    return SKStrokeJoin.Round;
                case StrokeJoin.Bevel:
                    return SKStrokeJoin.Bevel;
                default:
                    return SKStrokeJoin.Miter;
            }
        }
    }
}
