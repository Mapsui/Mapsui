using Windows.Foundation;

namespace Mapsui.UI.WinUI.Extensions

{
    public static class PointExtensions
    {
        public static Geometries.Point ToMapsui(this Point point)
        {
            return new Geometries.Point(point.X, point.Y);
        }
    }
}