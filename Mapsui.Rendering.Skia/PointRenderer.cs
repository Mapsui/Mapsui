using System;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    internal static class PointRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature,
            double x, double y, SymbolCache symbolCache, float opacity)
        {
            var (destX, destY) = viewport.WorldToScreenXY(x, y);

            if (style is CalloutStyle calloutStyle)
            {
                CalloutStyleRenderer.Draw(canvas, viewport, opacity, destX, destY, calloutStyle);
            }
            else if (style is LabelStyle labelStyle)
            {
                LabelRenderer.Draw(canvas, labelStyle, feature, destX, destY, opacity);
            }
            else if (style is SymbolStyle symbolStyle)
            {
                if (symbolStyle.BitmapId >= 0)
                {
                    // todo: Remove this call. ImageStyle should be used instead of SymbolStyle with BitmapId
                    ImageStyleRenderer.Draw(canvas, symbolStyle, destX, destY, symbolCache, opacity, viewport.Rotation);
                }
                else
                {
                    SymbolStyleRenderer.Draw(canvas, symbolStyle, destX, destY, opacity, symbolStyle.SymbolType, viewport.Rotation);
                }
            }
            else if (style is ImageStyle imageStyle)
            {
                ImageStyleRenderer.Draw(canvas, imageStyle, destX, destY, symbolCache, opacity, viewport.Rotation);
            }
            else if (style is VectorStyle vectorStyle)
            {
                // Use the SymbolStyleRenderer and specify Ellipse
                SymbolStyleRenderer.Draw(canvas, vectorStyle, destX, destY, opacity, SymbolType.Ellipse);
            }
            else
            {
                throw new Exception($"Style of type '{style.GetType()}' is not supported for points");
            }
        }
    }
}