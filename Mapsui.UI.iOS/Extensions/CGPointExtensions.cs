using CoreGraphics;

namespace Mapsui.UI.iOS
{
    static class CGPointExtensions
    {
        public static Geometries.Point ToMapsui(this CGPoint point)
        {
            return new Geometries.Point(point.X, point.Y);
        }
    }
}