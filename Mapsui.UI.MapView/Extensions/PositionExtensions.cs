using Mapsui.Projections;

namespace Mapsui.UI.Maui.Extensions;

public static class PositionExtensions
{
    /// <summary>
    /// Convert Mapsui.Geometries.Point to Mapsui.UI.Maui.Position
    /// </summary>
    /// <param name="point">Point in Mapsui format</param>
    /// <returns>Return a Position type</returns>
    public static Position ToMaui(this MPoint point)
    {
        return point.ToNative();
    }

    public static Position ToNative(this MPoint point)
    {
        var result = SphericalMercator.ToLonLat(point.X, point.Y);
        return new Position(result.lat, result.lon);
    }
}
