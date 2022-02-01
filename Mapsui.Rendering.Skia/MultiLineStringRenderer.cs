using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class MultiLineStringRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature,
            MultiLineString multiLineString, float opacity)
        {
            foreach (var geometry in multiLineString.Geometries)
            {
                var lineString = (LineString)geometry;
                LineStringRenderer.Draw(canvas, viewport, style, feature, lineString, opacity);
            }
        }
    }
}