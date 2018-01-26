using Mapsui.Geometries;
using Mapsui.Widgets.ScaleBar;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class ScaleBarWidgetRenderer
    {
        private const float strokeExternal = 4;
        private const float strokeInternal = 2;

        private static SKPaint paintScaleBar;
        private static SKPaint paintScaleBarStroke;
        private static SKPaint paintScaleText;
        private static SKPaint paintScaleTextStroke;

        public static void Draw(SKCanvas canvas, double screenWidth, double screenHeight, ScaleBarWidget scaleBar,
            float layerOpacity)
        {
            // If this widget belongs to no viewport, than stop drawing
            if (scaleBar.Viewport == null)
                return;

            // Things aren't correct anymore, so create them new
            paintScaleBar = CreateScaleBarPaint(scaleBar.TextColor.ToSkia(layerOpacity), strokeInternal, SKPaintStyle.Fill, scaleBar.Scale);
            paintScaleBarStroke = CreateScaleBarPaint(scaleBar.BackColor.ToSkia(layerOpacity), strokeExternal, SKPaintStyle.Stroke, scaleBar.Scale);
            paintScaleText = CreateTextPaint(scaleBar.TextColor.ToSkia(layerOpacity), 2, SKPaintStyle.Fill, scaleBar.Scale);
            paintScaleTextStroke = CreateTextPaint(scaleBar.BackColor.ToSkia(layerOpacity), 2, SKPaintStyle.Stroke, scaleBar.Scale);

            // TODO: Remove
            // Draw a rect around the scale bar for testing
            var tempPaint = new SKPaint() { StrokeWidth = 1, Color = SKColors.Blue, IsStroke = true };

            float scaleBarLength1;
            int mapScaleLength1;
            string mapScaleText1;

            (scaleBarLength1, mapScaleLength1, mapScaleText1) = scaleBar.CalculateScaleBarLengthAndValue(scaleBar.Viewport, scaleBar.MaxWidth);

            float scaleBarLength2;
            int mapScaleLength2;
            string mapScaleText2;

            if (scaleBar.ScaleBarMode == ScaleBarMode.Both)
            {
                (scaleBarLength2, mapScaleLength2, mapScaleText2) = scaleBar.CalculateScaleBarLengthAndValue(scaleBar.Viewport, scaleBar.MaxWidth, scaleBar.SecondaryUnitConverter);
            }
            else
            {
                (scaleBarLength2, mapScaleLength2, mapScaleText2) = (0, 0, null);
            }

            // Calc height of scale bar
            SKRect textSize;

            // Do this, because height of text changes sometimes (e.g. from 2 m to 1 m)
            paintScaleTextStroke.MeasureText("9999 m", ref textSize);

            var scaleBarHeight = textSize.Height + (scaleBar.TickLength + strokeExternal * 0.5f + scaleBar.TextMargin) * scaleBar.Scale;

            if (scaleBar.ScaleBarMode == ScaleBarMode.Both)
            {
                scaleBarHeight *= 2;
            }
            else
            {
                scaleBarHeight += strokeExternal * 0.5f * scaleBar.Scale;
            }

            scaleBar.Height = scaleBarHeight;

            // Get lines for scale bar
            var points = scaleBar.DrawScaleBar(scaleBarLength1, scaleBarLength2, strokeExternal);

            // BoundingBox for scale bar
            BoundingBox bb = new BoundingBox();

            if (points != null)
            {
                // Draw outline of scale bar
                for (int i = 0; i < points.Length; i += 2)
                {
                    canvas.DrawLine((float)points[i].X, (float)points[i].Y, (float)points[i + 1].X, (float)points[i + 1].Y, paintScaleBarStroke);
                }

                // Draw scale bar
                for (int i = 0; i < points.Length; i += 2)
                {
                    canvas.DrawLine((float)points[i].X, (float)points[i].Y, (float)points[i + 1].X, (float)points[i + 1].Y, paintScaleBar);
                }

                bb = points[0].GetBoundingBox();

                for (int i = 1; i < points.Length; i++)
                {
                    bb = bb.Join(points[i].GetBoundingBox());
                }

                bb = bb.Grow(strokeExternal * 0.5f * scaleBar.Scale);
            }

            // Draw text
            // Calc text height
            SKRect textSize1;
            SKRect textSize2;

            mapScaleText1 = mapScaleText1 ?? string.Empty;
            mapScaleText2 = mapScaleText2 ?? string.Empty;

            paintScaleTextStroke.MeasureText(mapScaleText1, ref textSize1);
            paintScaleTextStroke.MeasureText(mapScaleText2, ref textSize2);

            var (posX1, posY1, posX2, posY2) = scaleBar.DrawText(textSize.ToMapsui(), textSize1.ToMapsui(), textSize2.ToMapsui(), strokeExternal);

            // Now draw text
            canvas.DrawText(mapScaleText1, posX1, posY1 - textSize1.Top, paintScaleTextStroke);
            canvas.DrawText(mapScaleText1, posX1, posY1 - textSize1.Top, paintScaleText);

            bb = bb.Join(new BoundingBox(posX1, posY1, posX1 + textSize1.Width, posY1 + textSize1.Height));

            if (scaleBar.ScaleBarMode == ScaleBarMode.Both)
            {
                // Now draw text
                canvas.DrawText(mapScaleText2, posX2, posY2 - textSize2.Top, paintScaleTextStroke);
                canvas.DrawText(mapScaleText2, posX2, posY2 - textSize2.Top, paintScaleText);

                bb = bb.Join(new BoundingBox(posX2, posY2, posX2 + textSize2.Width, posY2 + textSize2.Height));
            }

            scaleBar.Envelope = bb;

            // TODO: Remove
            canvas.DrawRect(new SKRect((float)bb.MinX, (float)bb.MinY, (float)bb.MaxX, (float)bb.MaxY), tempPaint);
        }

        private static SKPaint CreateScaleBarPaint(SKColor color, float strokeWidth, SKPaintStyle style, float scale)
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