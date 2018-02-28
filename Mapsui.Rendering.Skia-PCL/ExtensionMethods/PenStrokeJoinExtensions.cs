using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class PenStrokeJoinExtensions
    {
        public static SKStrokeJoin ToSkia(this PenStrokeJoin penStrokeJoin)
        {
            switch (penStrokeJoin)
            {
                case PenStrokeJoin.Miter:
                    return SKStrokeJoin.Miter;
                case PenStrokeJoin.Round:
                    return SKStrokeJoin.Round;
                case PenStrokeJoin.Bevel:
                    return SKStrokeJoin.Bevel;
                default:
                    return SKStrokeJoin.Miter;
            }
        }
    }
}
