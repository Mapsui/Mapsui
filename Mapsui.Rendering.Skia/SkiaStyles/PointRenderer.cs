using System;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    internal static class PointRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature,
            double x, double y, ISymbolCache symbolCache, float opacity)
        {
            var (destX, destY) = viewport.WorldToScreenXY(x, y);

        }
    }
}