using Mapsui.Widgets.CenterCross;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class CenterCrossWidgetRenderer
    {
        private const float StrokeExternal = 4;
        private const float StrokeInternal = 2;

        private static SKPaint _paintCenterCrossInternal;
        private static SKPaint _paintCenterCrossExternal;

        public static void Draw(SKCanvas canvas, double screenWidth, double screenHeight, CenterCrossWidget centerCross,
            float layerOpacity)
        {
            // If this is the first time, we call this renderer, ...
            if (_paintCenterCrossInternal == null)
            {
                // ... than create the paints
                _paintCenterCrossInternal = CreateCenterCrossPaint(centerCross.Color.ToSkia(layerOpacity), StrokeInternal, SKPaintStyle.Fill, centerCross.Scale);
                _paintCenterCrossExternal = CreateCenterCrossPaint(centerCross.Color.ToSkia(layerOpacity), StrokeExternal, SKPaintStyle.Fill, centerCross.Scale);
            }
            else
            {
                // Update paints with new values
                _paintCenterCrossInternal.Color = centerCross.Color.ToSkia(layerOpacity);
                _paintCenterCrossInternal.StrokeWidth = StrokeInternal * centerCross.Scale;
                _paintCenterCrossExternal.Color = centerCross.Halo.ToSkia(layerOpacity);
                _paintCenterCrossExternal.StrokeWidth = StrokeExternal * centerCross.Scale;
            }

            var centerX = (float)centerCross.Map.Viewport.Width * 0.5f;
            var centerY = (float)centerCross.Map.Viewport.Height * 0.5f;
            var halfWidth = centerCross.Width * 0.5f + (StrokeExternal - StrokeInternal) * centerCross.Scale;
            var halfHeight = centerCross.Height * 0.5f;
            var haloSize = (StrokeExternal - StrokeInternal) * 0.5f * centerCross.Scale;

            canvas.DrawLine(centerX - halfWidth - haloSize, centerY, centerX + halfWidth + haloSize, centerY, _paintCenterCrossExternal);
            canvas.DrawLine(centerX, centerY - halfHeight - haloSize, centerX, centerY + halfHeight + haloSize, _paintCenterCrossExternal);
            canvas.DrawLine(centerX - halfWidth, centerY, centerX + halfWidth, centerY, _paintCenterCrossInternal);
            canvas.DrawLine(centerX, centerY - halfHeight, centerX, centerY + halfHeight, _paintCenterCrossInternal);
        }

        private static SKPaint CreateCenterCrossPaint(SKColor color, float strokeWidth, SKPaintStyle style, float scale)
        {
            SKPaint paint = new SKPaint();

            paint.LcdRenderText = true;
            paint.Color = color;
            paint.StrokeWidth = strokeWidth * scale;
            paint.Style = style;
            paint.StrokeCap = SKStrokeCap.Square;

            return paint;
        }

        private static SKPaint CreateTextPaint(SKColor color, float strokeWidth, SKPaintStyle style, float scale)
        {
            SKPaint paint = new SKPaint();

            paint.LcdRenderText = true;
            paint.Color = color;
            paint.StrokeWidth = strokeWidth * scale;
            paint.Style = style;
            paint.Typeface = SKTypeface.FromFamilyName("Arial", SKTypefaceStyle.Bold);
            paint.TextSize = 10 * scale;
            paint.IsAntialias = true;

            return paint;
        }
    }
}