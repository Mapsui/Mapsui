using System;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class PenStyleExtension
{
    public static SKPathEffect ToSkia(this PenStyle penStyle, float width, float[]? dashArray = null, float dashOffset = 0)
    {
        switch (penStyle)
        {
            case PenStyle.UserDefined:
                // If dashArray is empty or not even, create sold dash
                if (dashArray == null || dashArray.Length == 0 || dashArray.Length % 2 != 0)
                    return SKPathEffect.CreateDash(Array.Empty<float>(), 0);
                // Multiply each dash entry with line width
                var dash = new float[dashArray.Length];
                for (var i = 0; i < dashArray.Length; i++)
                    dash[i] = dashArray[i] * width;
                return SKPathEffect.CreateDash(dash, dashOffset);
            case PenStyle.Dash:
                return SKPathEffect.CreateDash(new[] { width * 4f, width * 3f }, dashOffset);
            case PenStyle.Dot:
                return SKPathEffect.CreateDash(new[] { width * 1f, width * 3f }, dashOffset);
            case PenStyle.DashDot:
                return SKPathEffect.CreateDash(new[] { width * 4f, width * 3f, width * 1f, width * 3f }, dashOffset);
            case PenStyle.DashDotDot:
                return SKPathEffect.CreateDash(new[] { width * 4f, width * 3f, width * 1f, width * 3f, width * 1f, width * 3f }, dashOffset);
            case PenStyle.LongDash:
                return SKPathEffect.CreateDash(new[] { width * 8f, width * 3f }, dashOffset);
            case PenStyle.LongDashDot:
                return SKPathEffect.CreateDash(new[] { width * 8f, width * 3f, width * 1f, width * 3f }, dashOffset);
            case PenStyle.ShortDash:
                return SKPathEffect.CreateDash(new[] { width * 2f, width * 3f }, dashOffset);
            case PenStyle.ShortDashDot:
                return SKPathEffect.CreateDash(new[] { width * 2f, width * 3f, width * 1f, width * 3f }, dashOffset);
            case PenStyle.ShortDashDotDot:
                return SKPathEffect.CreateDash(new[] { width * 2f, width * 3f, width * 1f, width * 3f, width * 1f, width * 3f }, dashOffset);
            case PenStyle.ShortDot:
                return SKPathEffect.CreateDash(new[] { width * 1f, width * 3f }, dashOffset);
            default:
                return SKPathEffect.CreateDash(Array.Empty<float>(), dashOffset);
        }
    }
}
