using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class MultiLineStringRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature, IGeometry geometry,
            float opacity)
        {
            var multiLineString = (MultiLineString) geometry;

            foreach (var lineString in multiLineString)
            {
                LineStringRenderer.Draw(canvas, viewport, style, feature, lineString, opacity);
            }
        }
    }
}