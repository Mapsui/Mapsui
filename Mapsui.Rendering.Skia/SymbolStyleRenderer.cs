using System;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public class SymbolStyleRenderer
    {
        public static void Draw(SKCanvas canvas, VectorStyle vectorStyle,
            double x, double y, float opacity, SymbolType symbolType)
        {
            canvas.Save();
            canvas.Translate((float)x, (float)y);
            Draw(canvas, vectorStyle, opacity, symbolType);
            canvas.Restore();
        }

        public static void Draw(SKCanvas canvas, SymbolStyle style,
            double x, double y, float opacity, SymbolType symbolType, double mapRotation)
        {
            canvas.Save();
            canvas.Translate((float)x, (float)y);
            canvas.Scale((float)style.SymbolScale, (float)style.SymbolScale);
            if (style.SymbolOffset.IsRelative)
                canvas.Translate((float)(SymbolStyle.DefaultWidth * style.SymbolOffset.X), (float)(-SymbolStyle.DefaultWidth * style.SymbolOffset.Y));
            else
                canvas.Translate((float)style.SymbolOffset.X, (float)-style.SymbolOffset.Y);
            if (style.SymbolRotation != 0)
            {
                var rotation = style.SymbolRotation;
                if (style.RotateWithMap) rotation += mapRotation;
                canvas.RotateDegrees((float)rotation);
            }

            Draw(canvas, style, opacity, symbolType);
            canvas.Restore();
        }

        public static void Draw(SKCanvas canvas, VectorStyle vectorStyle,
            float opacity, SymbolType symbolType = SymbolType.Ellipse)
        {
            var width = (float)SymbolStyle.DefaultWidth;
            var halfWidth = width / 2;
            var halfHeight = (float)SymbolStyle.DefaultHeight / 2;

            using var fillPaint = CreateFillPaint(vectorStyle.Fill, opacity);
            using var linePaint = CreateLinePaint(vectorStyle.Outline, opacity);

            switch (symbolType)
            {
                case SymbolType.Ellipse:
                    DrawCircle(canvas, 0, 0, halfWidth, fillPaint, linePaint);
                    break;
                case SymbolType.Rectangle:
                    var rect = new SKRect(-halfWidth, -halfHeight, halfWidth, halfHeight);
                    DrawRect(canvas, rect, fillPaint, linePaint);
                    break;
                case SymbolType.Triangle:
                    DrawTriangle(canvas, 0, 0, width, fillPaint, linePaint);
                    break;
                default: // Invalid value
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static SKPaint? CreateLinePaint(Pen? outline, float opacity)
        {
            if (outline is null) return null;

            return new SKPaint
            {
                Color = outline.Color.ToSkia(opacity),
                StrokeWidth = (float)outline.Width,
                StrokeCap = outline.PenStrokeCap.ToSkia(),
                PathEffect = outline.PenStyle.ToSkia((float)outline.Width),
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
        }

        private static SKPaint? CreateFillPaint(Brush? fill, float opacity)
        {
            if (fill is null) return null;

            return new SKPaint
            {
                Color = fill.Color.ToSkia(opacity),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        private static void DrawCircle(SKCanvas canvas, float x, float y, float radius, SKPaint? fillColor,
          SKPaint? lineColor)
        {
            if (fillColor != null && fillColor.Color.Alpha != 0) canvas.DrawCircle(x, y, radius, fillColor);
            if (lineColor != null && lineColor.Color.Alpha != 0) canvas.DrawCircle(x, y, radius, lineColor);
        }

        private static void DrawRect(SKCanvas canvas, SKRect rect, SKPaint? fillColor, SKPaint? lineColor)
        {
            if (fillColor != null && fillColor.Color.Alpha != 0) canvas.DrawRect(rect, fillColor);
            if (lineColor != null && lineColor.Color.Alpha != 0) canvas.DrawRect(rect, lineColor);
        }

        /// <summary>
        /// Equilateral triangle of side 'sideLength', centered on the same point as if a circle of diameter 'sideLength' was there
        /// </summary>
        private static void DrawTriangle(SKCanvas canvas, float x, float y, float sideLength, SKPaint? fillColor, SKPaint? lineColor)
        {
            var altitude = Math.Sqrt(3) / 2.0 * sideLength;
            var inradius = altitude / 3.0;
            var circumradius = 2.0 * inradius;

            var topX = x;
            var topY = y - circumradius;
            var leftX = x + sideLength * -0.5;
            var leftY = y + inradius;
            var rightX = x + sideLength * 0.5;
            var rightY = y + inradius;

            using var path = new SKPath();
            path.MoveTo(topX, (float)topY);
            path.LineTo((float)leftX, (float)leftY);
            path.LineTo((float)rightX, (float)rightY);
            path.Close();

            if ((fillColor != null) && fillColor.Color.Alpha != 0) canvas.DrawPath(path, fillColor);
            if ((lineColor != null) && lineColor.Color.Alpha != 0) canvas.DrawPath(path, lineColor);
        }
    }
}
