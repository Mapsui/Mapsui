using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

public static class LineStringRenderer
{
    public static void Draw(SKCanvas canvas, Viewport viewport, ILayer layer, VectorStyle? vectorStyle,
        IFeature feature, LineString lineString, float opacity, IRenderCache renderCache)
    {
        if (vectorStyle == null)
            return;

        var lineWidth = (float)(vectorStyle.Line?.Width ?? 1f);
        var extent = viewport.ToExtent();
        var rotation = viewport.Rotation;
        if (vectorStyle.Line.IsVisible())
        {
            var paint = renderCache.GetOrCreatePaint((vectorStyle.Line, opacity), CreateSkPaint);
            var path = renderCache.GetOrCreatePath((feature.Id, extent, rotation, lineWidth),
                f => lineString.ToSkiaPath(viewport, viewport.ToSkiaRect(), lineWidth));

            canvas.DrawPath(path, paint);
        }
    }

    private static SKPaint CreateSkPaint((Pen? pen, float opacity) valueTuple)
    {
        var pen = valueTuple.pen;
        var opacity = valueTuple.opacity;
        
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
