namespace Mapsui.UI.Forms.Extensions
{
    public static class ZoomLevelExtensions
    {
        /// <summary>
        /// Convert zoom level (as discribed at https://wiki.openstreetmap.org/wiki/Zoom_levels) into a Mapsui resolution
        /// </summary>
        /// <param name="zoomLevel">Zoom level</param>
        /// <returns>Resolution in Mapsui format</returns>
        public static double ToMapsuiResolution(this double zoomLevel)
        {
            if (zoomLevel < 0 || zoomLevel > 30)
                return 0;

            return 156543.03392 / System.Math.Pow(2,zoomLevel);
        }

        /// <summary>
        /// Convert zoom level (as discribed at https://wiki.openstreetmap.org/wiki/Zoom_levels) into a Mapsui resolution
        /// </summary>
        /// <param name="zoomLevel">Zoom level</param>
        /// <returns>Resolution in Mapsui format</returns>
        public static double ToMapsuiResolution(this float zoomLevel)
        {
            return ((double)zoomLevel).ToMapsuiResolution();
        }

        /// <summary>
        /// Convert zoom level (as discribed at https://wiki.openstreetmap.org/wiki/Zoom_levels) into a Mapsui resolution
        /// </summary>
        /// <param name="zoomLevel">Zoom level</param>
        /// <returns>Resolution in Mapsui format</returns>
        public static double ToMapsuiResolution(this int zoomLevel)
        {
            return ((double)zoomLevel).ToMapsuiResolution();
        }

        /// <summary>
        /// Convert Mapsui resolution to zoom level (as discribed at https://wiki.openstreetmap.org/wiki/Zoom_levels)
        /// </summary>
        /// <param name="resolution">Resolution in Mpsui format</param>
        /// <returns>Zoom level</returns>
        public static double ToZoomLevel(this double resolution)
        {
            if (resolution < 0 || resolution > 156543.03392)
                return -1;

            return System.Math.Log(156543.04 / resolution, 2);
        }
    }
}