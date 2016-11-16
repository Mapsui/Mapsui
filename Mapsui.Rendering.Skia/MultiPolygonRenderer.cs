using Mapsui.Geometries;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class MultiPolygonRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IGeometry geometry)
        {
            var multiPolygon = (MultiPolygon) geometry;

            foreach (var polygon in multiPolygon)
            {
                PolygonRenderer.Draw(canvas, viewport, style, polygon);
            }
        }
    }
}