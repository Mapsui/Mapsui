using Android.Graphics;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Rendering.Android.ExtensionMethods;
using Mapsui.Styles;

namespace Mapsui.Rendering.Android
{
    static class LineStringRenderer
    {
        public static void Draw(Canvas canvas, IViewport viewport, IStyle style, IFeature feature)
        {
            var lineString = ((LineString) feature.Geometry).Vertices;
            var paints = style.ToAndroid();
            //using (var paint = new Paint {Color = Color.Black, StrokeWidth = 8, AntiAlias = true})
            foreach (var paint in paints)
            {
                var vertices = lineString;
                var points = vertices.ToAndroid();
                WorldToScreen(viewport, points);
                canvas.DrawLines(points, paint);
                paint.Dispose();
            }
        }

        private static void WorldToScreen(IViewport viewport, float[] points)
        {
            for (var i = 0; i < points.Length / 2; i++)
            {
                var point = viewport.WorldToScreen(points[i * 2], points[i * 2 + 1]);
                points[i * 2] = (float)point.X;
                points[i * 2 + 1] = (float)point.Y;
            }
        }
    }
}