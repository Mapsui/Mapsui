using BruTile.Predefined;
using System;

namespace MarinerNotices.MapsuiBuilder.Utilities;

public static class ZoomLevels
{
    private static readonly GlobalSphericalMercator _tileSchema = new();

    public static double GetResolutionOfLevel(int zoomLevel)
    {
        return _tileSchema.Resolutions[zoomLevel].UnitsPerPixel;
    }

    public static double GetResolutionBetweenThisAndMoreZoomedOutLevel(int moreZoomedInLevel)
    {
        double current = _tileSchema.Resolutions[moreZoomedInLevel].UnitsPerPixel;
        double moreZoomedOut = current * 2;

        // Geometric mean for powers-of-two steps
        return Math.Sqrt(current * moreZoomedOut);
    }

    public static int GetNearestLevel(double resolution)
    {
        return BruTile.Utilities.GetNearestLevel(_tileSchema.Resolutions, resolution);
    }
}
