using System;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Rendering.Skia.ExtensionMethods;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    static class CircleRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature, 
            IGeometry geometry, SymbolCache symbolCache, float layerOpacity)
        {
            var circle = geometry as Circle;
            var destination = viewport.WorldToScreen(new Point(circle.X, circle.Y));

            // Get current position
            var position = Projection.SphericalMercator.ToLonLat(circle.X, circle.Y);

            // Calc ground resolution in meters per pixel of viewport for this latitude
            double groundResolution = viewport.Resolution * Math.Cos(position.Y / 180.0 * Math.PI);

            // Now we can calc the radius of circle
            var radius = circle.Radius / groundResolution;

            if (style is VectorStyle)
            {
                var vectorStyle = style as VectorStyle;

                canvas.Save();
                canvas.Translate((float)destination.X, (float)destination.Y);

                var halfWidth = (float)radius / 2f;
                var halfHeight = halfWidth;

                var fillPaint = CreateFillPaint(vectorStyle.Fill, layerOpacity);
                var linePaint = CreateLinePaint(vectorStyle.Outline, layerOpacity);

                DrawCircle(canvas, 0, 0, halfWidth, fillPaint, linePaint);

                canvas.Restore();
            }
            else
            {
                throw new Exception($"Style of type '{style.GetType()}' is not supported for circle");
            }
        }

        private static SKPaint CreateLinePaint(Pen outline, float layerOpacity)
        {
            if (outline == null) return null;

            return new SKPaint
            {
                Color = outline.Color.ToSkia(layerOpacity),
                StrokeWidth = (float) outline.Width,
                StrokeCap = outline.PenStrokeCap.ToSkia(),
                PathEffect = outline.PenStyle.ToSkia((float)outline.Width),
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
        }

        private static SKPaint CreateFillPaint(Brush fill, float layerOpacity)
        {
            if (fill == null) return null;

            return new SKPaint
            {
                Color = fill.Color.ToSkia(layerOpacity),
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