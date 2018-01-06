using Mapsui.Geometries;

namespace Mapsui.UI
{
    public static class SharedMapControl
    {
        public static Point WorldToScreen(IViewport viewport, float scale, Point worldPosition)
        {
            var screenPosition = viewport.WorldToScreen(worldPosition);
            return new Point(screenPosition.X * scale, screenPosition.Y * scale);
        }

        public static Point ScreenToWorld(IViewport viewport, float scale, Point screenPosition)
        {
            var worldPosition = viewport.ScreenToWorld(screenPosition.X * scale, screenPosition.Y * scale);
            return new Point(worldPosition.X, worldPosition.Y);
        }


    }
}
