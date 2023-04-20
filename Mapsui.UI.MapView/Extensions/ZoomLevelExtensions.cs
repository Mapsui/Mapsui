namespace Mapsui.UI.Extensions;

public static class ZoomLevelExtensions
{
    /// <summary>
    /// Convert zoom level (as described at https://wiki.openstreetmap.org/wiki/Zoom_levels) into a Mapsui resolution
    /// </summary>
    /// <param name="zoomLevel">Zoom level</param>
    /// <returns>Resolution in Mapsui format</returns>
    public static double ToMapsuiResolution(this double zoomLevel)
    {
        if (zoomLevel < 0 || zoomLevel > 30)
            return 0;

        return 156543.03392 / System.Math.Pow(2, zoomLevel);
    }
}
