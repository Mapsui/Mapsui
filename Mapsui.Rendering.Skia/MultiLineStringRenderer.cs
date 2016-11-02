using Mapsui.Geometries;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class MultiLineStringRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IGeometry geometry)
        {
            var multiLineString = (MultiLineString)geometry;

            foreach (var lineString in multiLineString)
            {
                LineStringRenderer.Draw(canvas, viewport, style, lineString);
            }
        }
    }
}
