﻿using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class MultiPointRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature,
            MultiPoint multiPoint, SymbolCache symbolCache, float opacity)
        {
            foreach (var geometry in multiPoint)
            {
                var point = (Point)geometry;
                PointRenderer.Draw(canvas, viewport, style, feature, point.X, point.Y, symbolCache, opacity);
            }
        }
    }
}