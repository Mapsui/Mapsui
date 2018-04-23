using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class PenStyleExtension
    {
        public static SKPathEffect ToSkia(this PenStyle penStyle, float width, float[] dashArray = null)
        {
            switch (penStyle)
            {
                case PenStyle.UserDefined:
                    // If dashArray is empty or not even, create sold dash
                    if (dashArray == null || dashArray.Length == 0 || dashArray.Length % 2 != 0)
                        return SKPathEffect.CreateDash(new float[0], 0);
                    // Multiply each dash entry with line width
                    float[] dash = new float[dashArray.Length];
                    for (var i = 0; i < dashArray.Length; i++)
                    {
                        dash[i] = dashArray[i] * width;
                    }
                    return SKPathEffect.CreateDash(dash, 0);
                case PenStyle.Dash:
                    return SKPathEffect.CreateDash(new [] { width * 4f, width * 3f }, 0);
                case PenStyle.Dot:
                    return SKPathEffect.CreateDash(new [] { width * 1f, width * 3f }, 0);
                case PenStyle.DashDot:
                    return SKPathEffect.CreateDash(new [] { width * 4f, width * 3f, width * 1f, width * 3f }, 0);
                case PenStyle.DashDotDot:
                    return SKPathEffect.CreateDash(new [] { width * 4f, width * 3f, width * 1f, width * 3f, width * 1f, width * 3f }, 0);
                case PenStyle.LongDash:
                    return SKPathEffect.CreateDash(new [] { width * 8f, width * 3f }, 0);
                case PenStyle.LongDashDot:
                    return SKPathEffect.CreateDash(new [] { width * 8f, width * 3f, width * 1f, width * 3f }, 0);
                case PenStyle.ShortDash:
                    return SKPathEffect.CreateDash(new [] { width * 2f, width * 3f }, 0);
                case PenStyle.ShortDashDot:
                    return SKPathEffect.CreateDash(new [] { width * 2f, width * 3f, width * 1f, width * 3f }, 0);
                case PenStyle.ShortDashDotDot:
                    return SKPathEffect.CreateDash(new [] { width * 2f, width * 3f, width * 1f, width * 3f, width * 1f, width * 3f }, 0);
                case PenStyle.ShortDot:
                    return SKPathEffect.CreateDash(new [] { width * 1f, width * 3f }, 0);
                default:
                    return SKPathEffect.CreateDash(new float[0], 0);
            }
        }
    }
}
