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
        IVectorCache vectorCache)
    {
        SKPath ToPath((GeometryCollection collection, IFeature feature, Viewport viewport, float lineWidth) valueTuple)
        {
            var skRect = vectorCache.GetOrCreatePath(valueTuple.viewport, Extensions.ViewportExtensions.ToSkiaRect);
            var result = collection.ToSkiaPath(valueTuple.viewport, skRect, valueTuple.lineWidth);
            _ = result.Bounds;
            return result;
        }
        
        if (vectorStyle == null)
            return;

        float lineWidth = (float)(vectorStyle.Outline?.Width ?? 1f);
        if (vectorStyle.Fill.IsVisible())
        {
            var paintFill = vectorCache.GetOrCreatePaint((vectorStyle.Fill, opacity, viewport.Rotation), PolygonRenderer.CreateSkPaint);
            var path = vectorCache.GetOrCreatePath((collection, feature, viewport, lineWidth), ToPath); 
            canvas.DrawPath(path, paintFill);
        }

        if (vectorStyle.Outline.IsVisible())
        {
            var paint = vectorCache.GetOrCreatePaint((vectorStyle.Outline, opacity), PolygonRenderer.CreateSkPaint);
            var path = vectorCache.GetOrCreatePath((collection, feature, viewport, lineWidth), ToPath); 
            canvas.DrawPath(path, paint);
        }
    }
}
