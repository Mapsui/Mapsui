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

            if (style is CalloutStyle calloutStyle)
            {
                CalloutStyleRenderer.Draw(canvas, viewport, opacity, destX, destY, calloutStyle);
            }
            else
            {
                //throw new Exception($"Style of type '{style.GetType()}' is not supported for points");
            }
        }
    }
}