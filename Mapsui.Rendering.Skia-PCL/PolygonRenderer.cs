using System.Linq;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    internal static class PolygonRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature, IGeometry geometry,
            float opacity, SymbolCache symbolCache = null)
        {
            var polygon = (Polygon)geometry;
            var path = ToSkiaPath(polygon, viewport);
            canvas.DrawPath(path, new SKPaint{ Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 2, Color = SKColors.Red});
            path.Dispose();
        }

        public static SKPath ToSkiaPath(this Polygon polygon, IViewport viewport)
        {
            var exterior = polygon.ExteriorRing.Vertices.Select(v =>
            {
                var p = viewport.WorldToScreen(v);
                return new SKPoint((float) p.X, (float) p.Y);
            }).ToList();

            var path = new SKPath();

            path.MoveTo(exterior[0].X, exterior[0].Y);

            for (var i = 1; i <exterior.Count; i++)
            {
                path.LineTo(exterior[i].X, exterior[i].Y);
            }

            path.Close();
            
            return path;
        }
    }
}