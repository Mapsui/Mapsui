using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class MultiPointRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature,
            IGeometry geometry, SymbolCache symbolCache, float opacity)
        {
            var multiPoint = (MultiPoint) geometry;

            foreach (var point in multiPoint)
            {
                PointRenderer.Draw(canvas, viewport, style, feature, point, symbolCache, opacity);
            }
        }
    }
}