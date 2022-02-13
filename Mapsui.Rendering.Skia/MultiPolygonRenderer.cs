using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class MultiPolygonRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature,
            MultiPolygon multiPolygon, float opacity, ISymbolCache? symbolCache = null)
        {
            foreach (var geometry in multiPolygon.Geometries)
            {
                var polygon = (Polygon)geometry;
                PolygonRenderer.Draw(canvas, viewport, style, feature, polygon, opacity, symbolCache);
            }
        }
    }
}