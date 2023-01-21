using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Extensions;

public static class TupleExtensions
{
    public static Coordinate ToCoordinate(this (double x, double y) coordinate)
    {
        return new Coordinate(coordinate.x, coordinate.y);
    }
}
