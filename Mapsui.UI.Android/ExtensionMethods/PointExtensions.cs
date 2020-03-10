using Mapsui.Geometries;

namespace Mapsui.UI.Android
{
    public static class PointExtensions
    {
        public static Point ToDeviceIndependentUnits(this Point point, float pixelDensity)
        {
            return new Point(
                ToDeviceIndependentUnits(point.X, pixelDensity), 
                ToDeviceIndependentUnits(point.Y, pixelDensity));
        }
        
        private static double ToDeviceIndependentUnits(double devicePixels, float pixelDensity)
        {
            return devicePixels / pixelDensity;
        }
    }
}