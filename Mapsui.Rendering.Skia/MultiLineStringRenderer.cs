using Mapsui.Geometries;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class MultiLineStringRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature,
            MultiLineString multiLineString, float opacity)
        {
            foreach (var lineString in multiLineString.LineStrings)
                LineStringRenderer.Draw(canvas, viewport, style, feature, lineString, opacity);
        }
    }
}