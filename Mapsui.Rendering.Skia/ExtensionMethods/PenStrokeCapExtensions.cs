using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class PenStrokeCapExtensions
    {
        public static SKStrokeCap ToSkia(this PenStrokeCap penStrokeCap)
        {
            switch (penStrokeCap)
            {
                case PenStrokeCap.Butt:
                    return SKStrokeCap.Butt;
                case PenStrokeCap.Round:
                    return SKStrokeCap.Round;
                case PenStrokeCap.Square:
                    return SKStrokeCap.Square;
                default:
                    return SKStrokeCap.Butt;
            }
        }
    }
}
