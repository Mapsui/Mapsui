using System;
using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaStyles;

public static class GeometryCollectionRenderer
{
    public static void Draw(
        SKCanvas canvas,
        Viewport viewport,
        VectorStyle? vectorStyle,
        IFeature feature,
        GeometryCollection collection,
        float opacity,
        IVectorCache<SKPath, SKPaint> vectorCache)
    {
        SKPath ToPath((GeometryCollection collection, IFeature feature, Viewport viewport, float lineWidth) valueTuple)
        {
            var result = collection.ToSkiaPath(valueTuple.viewport, valueTuple.viewport.ToSkiaRect(), valueTuple.lineWidth);
            return result;
        }

        if (vectorStyle == null)
            return;

        float lineWidth = (float)(vectorStyle.Outline?.Width ?? 1f);
        using var path = vectorCache.GetOrCreatePath((collection, feature, viewport, lineWidth), ToPath);
        if (vectorStyle.Fill.IsVisible())
        {
            using var paintFill = vectorCache.GetOrCreatePaint((vectorStyle.Fill, opacity, viewport.Rotation), PolygonRenderer.CreateSkPaint);
            PolygonRenderer.DrawPath(canvas, vectorStyle, path, paintFill);
        }

        if (vectorStyle.Outline.IsVisible())
        {
            using var paint = vectorCache.GetOrCreatePaint((vectorStyle.Outline, opacity), PolygonRenderer.CreateSkPaint);
            canvas.DrawPath(path, paint);
        }
    }
}
