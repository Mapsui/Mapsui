using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.ExtensionMethods
{
    public static class PenStyleExtension
    {
        public static SKPathEffect ToSkia(this PenStyle penStyle, float width)
        {
            switch (penStyle)
            {
                case PenStyle.Dash:
                    return SKPathEffect.CreateDash(new float[2] { width * 4f, width * 3f }, 0);
                case PenStyle.Dot:
                    return SKPathEffect.CreateDash(new float[2] { width * 1f, width * 3f }, 0);
                case PenStyle.DashDot:
                    return SKPathEffect.CreateDash(new float[4] { width * 4f, width * 3f, width * 1f, width * 3f }, 0);
                case PenStyle.DashDotDot:
                    return SKPathEffect.CreateDash(new float[6] { width * 4f, width * 3f, width * 1f, width * 3f, width * 1f, width * 3f }, 0);
                case PenStyle.LongDash:
                    return SKPathEffect.CreateDash(new float[2] { width * 8f, width * 3f }, 0);
                case PenStyle.LongDashDot:
                    return SKPathEffect.CreateDash(new float[4] { width * 8f, width * 3f, width * 1f, width * 3f }, 0);
                case PenStyle.ShortDash:
                    return SKPathEffect.CreateDash(new float[2] { width * 2f, width * 3f }, 0);
                case PenStyle.ShortDashDot:
                    return SKPathEffect.CreateDash(new float[4] { width * 2f, width * 3f, width * 1f, width * 3f }, 0);
                case PenStyle.ShortDashDotDot:
                    return SKPathEffect.CreateDash(new float[6] { width * 2f, width * 3f, width * 1f, width * 3f, width * 1f, width * 3f }, 0);
                case PenStyle.ShortDot:
                    return SKPathEffect.CreateDash(new float[2] { width * 1f, width * 3f }, 0);
                default:
                    return SKPathEffect.CreateDash(new float[0], 0);
            }
        }
    }
}
