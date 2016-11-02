using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class MultiPointRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature, 
            IGeometry geometry, IDictionary<int, SKBitmapInfo> symbolBitmapCache)
        {
            var multiPoint = (MultiPoint)geometry;

            foreach (var point in multiPoint)
            {
                PointRenderer.Draw(canvas, viewport, style, feature, point, symbolBitmapCache);
            }
        }
    }
}