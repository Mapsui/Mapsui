using Android.Graphics;
using Mapsui.Providers;
using Mapsui.Styles;
using Point = Mapsui.Geometries.Point;
using Mapsui.Rendering.Android.ExtensionMethods;

namespace Mapsui.Rendering.Android
{
    static class PointRenderer
    {
        public static void Draw(Canvas canvas, IViewport viewport, IStyle style, IFeature feature)
        {
            var point = feature.Geometry as Point;
            var dest = viewport.WorldToScreen(point);

            var paints = style.ToAndroid();
            foreach (var paint in paints)
            {
                canvas.DrawCircle((int) dest.X, (int) dest.Y, 20, paint);
                paint.Dispose();
            }
        }
    }
}