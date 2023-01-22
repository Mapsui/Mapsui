using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Extensions;

public static class PointExtensions
{
    public static MPoint ToMPoint(this Point point)
    {
        return new MPoint(point.X, point.Y);
    }
}
