using System;
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
            var skRect = vectorCache.GetOrCreatePath(valueTuple.viewport, ViewportExtensions.ToSkiaRect);
            return collection.ToSkiaPath(valueTuple.viewport, skRect, valueTuple.lineWidth);
        }
        
        if (vectorStyle == null)
            return;

        var paintFill = vectorCache.GetOrCreatePaint((vectorStyle.Fill, opacity, viewport.Rotation), PolygonRenderer.CreateSkPaint);
        float lineWidth = (float)(vectorStyle.Outline?.Width ?? 1f);
        if (paintFill.IsVisible())
        {
            var path = vectorCache.GetOrCreatePath((collection, feature, viewport, lineWidth), ToPath); 
            canvas.DrawPath(path, paintFill);
        }

        var paint = vectorCache.GetOrCreatePaint((vectorStyle.Outline, opacity), PolygonRenderer.CreateSkPaint);
        if (paint.IsVisible())
        {
            var path = vectorCache.GetOrCreatePath((collection, feature, viewport, lineWidth), ToPath); 
            canvas.DrawPath(path, paintFill);
        }
    }
}
