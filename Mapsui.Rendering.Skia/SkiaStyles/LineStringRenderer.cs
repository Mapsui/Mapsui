using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class LineStringRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, VectorStyle vectorStyle,
            LineString lineString, float opacity)
        {
            if (vectorStyle == null)
                return;

            float lineWidth = 1;
            var lineColor = new Color();

            var strokeCap = PenStrokeCap.Butt;
            var strokeJoin = StrokeJoin.Miter;
            var strokeMiterLimit = 4f;
            var strokeStyle = PenStyle.Solid;
            float[]? dashArray = null;
            float dashOffset = 0;

            if (vectorStyle.Line != null)
            {
                lineWidth = (float)vectorStyle.Line.Width;
                lineColor = vectorStyle.Line.Color;
                strokeCap = vectorStyle.Line.PenStrokeCap;
                strokeJoin = vectorStyle.Line.StrokeJoin;
                strokeMiterLimit = vectorStyle.Line.StrokeMiterLimit;
                strokeStyle = vectorStyle.Line.PenStyle;
                dashArray = vectorStyle.Line.DashArray;
                dashOffset = vectorStyle.Line.DashOffset;
            }

            using var path = lineString.ToSkiaPath(viewport, canvas.LocalClipBounds);
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
            canvas.DrawPath(path, paint);
        }
    }
}