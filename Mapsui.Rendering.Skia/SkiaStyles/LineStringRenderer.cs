using System;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ViewportExtensions = Mapsui.Rendering.Skia.Extensions.ViewportExtensions;

namespace Mapsui.Rendering.Skia;

public static class LineStringRenderer
{
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    public static void Draw(SKCanvas canvas, Viewport viewport, ILayer layer, VectorStyle? vectorStyle,
        IFeature feature, LineString lineString, float opacity, IVectorCache? vectorCache)
    {
        if (vectorStyle == null)
            return;

        SKPaint? paint;
        SKPath path;
        var lineWidth = Convert.ToSingle(vectorStyle.Line?.Width ?? 1);
        if (vectorCache == null)
        {
            paint = CreateSkPaint(vectorStyle.Line, opacity);
            path = lineString.ToSkiaPath(viewport, canvas.LocalClipBounds, lineWidth);
        }
        else
        {
            paint = vectorCache.GetOrCreatePaint(vectorStyle.Line, opacity, CreateSkPaint);
            path = vectorCache.GetOrCreatePath(viewport, feature, lineString, lineWidth,
                (geometry, vp, _) => geometry.ToSkiaPath(vp, vp.ToSkiaRect(), lineWidth));
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
