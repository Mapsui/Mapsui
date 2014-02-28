using Android.Graphics;
using Mapsui.Providers;
using Mapsui.Styles;
using Color = Android.Graphics.Color;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.Android
{
    static class PointRenderer
    {
        public static void Draw(Canvas canvas, IViewport viewport, IStyle style, IFeature feature)
        {
            var point = feature.Geometry as Point;
            var dest = viewport.WorldToScreen(point);
            canvas.DrawCircle((int)dest.X, (int)dest.Y, 20, new Paint { Color = Color.Blue });
        }
    }
}