using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Extensions;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.SkiaStyles;

public static class LineStringRenderer
{
    public static void Draw(SKCanvas canvas, Viewport viewport, VectorStyle? vectorStyle,
        IFeature feature, LineString lineString, float opacity, int position)
    {
        if (vectorStyle == null)
            return;

        if (vectorStyle.Line.IsVisible())
        {
            using var path = lineString.ToSkiaPath(viewport);

            // If the Outline property is set and has a width greater than 0, draw the outline first.
            if (vectorStyle.Outline?.Width > 0)
            {
                var width = vectorStyle.Outline.Width + vectorStyle.Outline.Width + vectorStyle.Line?.Width ?? 1;
                using var paintOutline = CreateSkPaint((vectorStyle.Outline, (float?)width, opacity));
                canvas.DrawPath(path, paintOutline);
            }

            using var paintLine = CreateSkPaint((vectorStyle.Line, (float?)null, opacity));
            canvas.DrawPath(path, paintLine);
        }
    }

    internal static SKPaint CreateSkPaint((Pen? pen, float? width, float opacity) valueTuple)
    {
        var pen = valueTuple.pen;
        var opacity = valueTuple.opacity;

        float lineWidth = valueTuple.width ?? 1;
        var lineColor = new Color();

        var strokeCap = PenStrokeCap.Butt;
        var strokeJoin = StrokeJoin.Miter;
        var strokeMiterLimit = 4f;
        var strokeStyle = PenStyle.Solid;
        float[]? dashArray = null;
        float dashOffset = 0;

        if (pen != null)
        {
            lineWidth = valueTuple.width ?? (float)pen.Width;
            lineColor = pen.Color;
            strokeCap = pen.PenStrokeCap;
            strokeJoin = pen.StrokeJoin;
            strokeMiterLimit = pen.StrokeMiterLimit;
            strokeStyle = pen.PenStyle;
            dashArray = pen.DashArray;
            dashOffset = pen.DashOffset;
        }

        var paint = new SKPaint { IsAntialias = true };
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
