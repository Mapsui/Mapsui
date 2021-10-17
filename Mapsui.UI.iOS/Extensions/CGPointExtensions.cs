using CoreGraphics;

namespace Mapsui.UI.iOS.Extensions
{
    static class CGPointExtensions
    {
        public static Geometries.Point ToMapsui(this CGPoint point)
        {
            return new Geometries.Point(point.X, point.Y);
        }
    }
}