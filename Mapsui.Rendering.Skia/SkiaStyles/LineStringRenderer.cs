using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class LineStringRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, VectorStyle? vectorStyle,
            LineString lineString, float opacity, IVectorCache vectorCache)
        {
            if (vectorStyle == null)
                return;

            var paint = vectorCache.GetOrCreatePaint(vectorStyle.Line, opacity, createSkPaint);
            var path = vectorCache.GetOrCreatePath(viewport, lineString, (geometry, viewport) =>
            {
                // TODO handle local clip bounds in caching and don't take it from the canvas.
                return lineString.ToSkiaPath(viewport, canvas.LocalClipBounds);
            });

            canvas.DrawPath(path, paint);
        }

        private static SKPaint createSkPaint(Pen? pen, float opacity)
        {
            float lineWidth = 1;
            var lineColor = new Color();

            var strokeCap = PenStrokeCap.Butt;
            var strokeJoin = StrokeJoin.Miter;
            var strokeMiterLimit = 4f;
            var strokeStyle = PenStyle.Solid;
            float[]? dashArray = null;
            float dashOffset = 0;

            if (pen != null)
            {
                lineWidth = (float)pen.Width;
                lineColor = pen.Color;
                strokeCap = pen.PenStrokeCap;
                strokeJoin = pen.StrokeJoin;
                strokeMiterLimit = pen.StrokeMiterLimit;
                strokeStyle = pen.PenStyle;
                dashArray = pen.DashArray;
                dashOffset = pen.DashOffset;
            }

            using var paint = new SKPaint { IsAntialias = true };
            paint.IsStroke = true;
            paint.StrokeWidth = lineWidth;
            paint.Color = lineColor.ToSkia(opacity);
            paint.StrokeCap = strokeCap.ToSkia();
            paint.StrokeJoin = strokeJoin.ToSkia();
            paint.StrokeMiter = strokeMiterLimit;
            paint.PathEffect = strokeStyle != PenStyle.Solid
                ? strokeStyle.ToSkia(lineWidth, dashArray, dashOffset)
                : null;
            return paint;
        }
    }
}