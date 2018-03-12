using System;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    static class CircleRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature, 
            IGeometry geometry, SymbolCache symbolCache, float opacity)
        {
            var circle = geometry as Circle;
            var destination = viewport.WorldToScreen(new Point(circle.X, circle.Y));
            var radius = Math.Abs(destination.X - viewport.WorldToScreen(new Point(circle.X + circle.Radius, circle.Y)).X);

            if (style is VectorStyle)
            {
                var vectorStyle = style as VectorStyle;

                canvas.Save();
                canvas.Translate((float)destination.X, (float)destination.Y);

                var halfWidth = (float)radius;
                var halfHeight = halfWidth;

                var fillPaint = CreateFillPaint(vectorStyle.Fill, opacity);
                var linePaint = CreateLinePaint(vectorStyle.Outline, opacity);

                DrawCircle(canvas, 0, 0, halfWidth, fillPaint, linePaint);

                canvas.Restore();
            }
            else
            {
                throw new Exception($"Style of type '{style.GetType()}' is not supported for circle");
            }
        }

        private static SKPaint CreateLinePaint(Pen outline, float opacity)
        {
            if (outline == null) return null;

            return new SKPaint
            {
                Color = outline.Color.ToSkia(opacity),
                StrokeWidth = (float) outline.Width,
                StrokeCap = outline.PenStrokeCap.ToSkia(),
                PathEffect = outline.PenStyle.ToSkia((float)outline.Width),
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
        }

        private static SKPaint CreateFillPaint(Brush fill, float opacity)
        {
            if (fill == null) return null;

            return new SKPaint
            {
                Color = fill.Color.ToSkia(opacity),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        private static void DrawCircle(SKCanvas canvas, float x, float y, float radius, SKPaint fillColor,
            SKPaint lineColor)
        {
            if (fillColor != null && fillColor.Color.Alpha != 0) canvas.DrawCircle(x, y, radius, fillColor);
            if (lineColor != null && lineColor.Color.Alpha != 0) canvas.DrawCircle(x, y, radius, lineColor);
        }
    }
}