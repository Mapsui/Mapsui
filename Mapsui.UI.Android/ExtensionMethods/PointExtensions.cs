using Mapsui.Geometries;

namespace Mapsui.UI.Android
{
    public static class PointExtensions
    {
        public static Point ToDip(this Point point, float scale)
        {
            return new Point(ToDip(point.X, scale), ToDip(point.Y, scale));
        }


        /// <summary>
        /// This method converts device pixels to Device Independent Units.
        /// When to use? In native Android touch positions are in device pixels
        /// whereas the skia canvas needs to be drawn in device independent units.
        /// If not labels on raster tiles will be unreadable  and symbols will be too small. 
        /// </summary>
        /// <returns>The devicePixels argument translated to Device Independent Units.</returns>
        private static double ToDip(double devicePixels, float scale)
        {
            return devicePixels / scale;
        }
    }
}