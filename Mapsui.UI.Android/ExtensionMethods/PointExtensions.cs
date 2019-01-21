using Mapsui.Geometries;

namespace Mapsui.UI.Android
{
    public static class PointExtensions
    {
        public static Point ToDeviceIndependentUnits(this Point point, float pixelsPerDeviceIndependentUnit)
        {
            return new Point(
                ToDeviceIndependentUnits(point.X, pixelsPerDeviceIndependentUnit), 
                ToDeviceIndependentUnits(point.Y, pixelsPerDeviceIndependentUnit));
        }
        
        private static double ToDeviceIndependentUnits(double devicePixels, float pixelsPerDeviceIndependentUnit)
        {
            return devicePixels / pixelsPerDeviceIndependentUnit;
        }
    }
}