using System;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

public static class LineStringRenderer
{
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, VectorStyle? vectorStyle,
        LineString lineString, float opacity, IVectorCache? vectorCache = null)
    {
        if (vectorStyle == null)
            return;

        SKPaint paint;
        SKPath path;
        if (vectorCache == null)
        {
            paint = CreateSkPaint(vectorStyle.Line, opacity);
            path = lineString.ToSkiaPath(viewport, canvas.LocalClipBounds);
        }
        else
        {
            paint = vectorCache.GetOrCreatePaint(vectorStyle.Line, opacity, CreateSkPaint);

            var lineWidth = Convert.ToSingle(vectorStyle.Line?.Width ?? 1);
            path = vectorCache.GetOrCreatePath(viewport, lineString, lineWidth, (geometry, viewport, _) => geometry.ToSkiaPath(viewport, viewport.ToSkiaRect()));
        }

        canvas.DrawPath(path, paint);
    }

    private static SKPaint CreateSkPaint(Pen? pen, float opacity)
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
