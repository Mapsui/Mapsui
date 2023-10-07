using System;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Rendering.Skia.SkiaStyles;

public static class GeometryCollectionRenderer
{
    public static void Draw(SKCanvas canvas, Viewport viewport, ILayer layer, VectorStyle? vectorStyle, IFeature feature,
        GeometryCollection collection, float opacity, IVectorCache vectorCache)
    {
        if (vectorStyle == null)
            return;

        var paint = vectorCache.GetOrCreatePaint<SKPaint, Pen>(vectorStyle.Outline, opacity, PolygonRenderer.CreateSkPaint);
        var paintFill = vectorCache.GetOrCreatePaint<SKPaint>(vectorStyle.Fill, opacity, viewport.Rotation, PolygonRenderer.CreateSkPaint);

        float lineWidth = Convert.ToSingle(vectorStyle.Outline?.Width ?? 1);
        var path = vectorCache.GetOrCreatePath(viewport, feature, collection, lineWidth,
            (collection, viewport, lineWidth) =>
        {
            var skRect = vectorCache.GetOrCreatePath(viewport, ViewportExtensions.ToSkiaRect);
            return collection.ToSkiaPath(viewport, skRect, lineWidth);
        });

        PolygonRenderer.DrawPath(canvas, vectorStyle, path, paintFill, paint);
    }
}
