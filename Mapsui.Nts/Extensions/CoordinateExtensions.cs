using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Extensions;

public static class CoordinateExtensions
{
    public static MPoint ToMPoint(this Coordinate coordinate)
    {
        return new MPoint(coordinate.X, coordinate.Y);
    }

    public static Point ToPoint(this Coordinate coordinate)
    {
        return new Point(coordinate.X, coordinate.Y);
    }

    public static LineString ToLineString(this IEnumerable<Coordinate> coordinates)
    {
        var list = coordinates.ToList();
        if (list.Count == 1)
            list.Add(list[0].Copy());
        return new LineString(list.ToArray());
    }

    public static LinearRing ToLinearRing(this IEnumerable<Coordinate> coordinates)
    {
        if (coordinates.Count() == 0)
            throw new Exception("coordinates can not be length 0");

        var list = coordinates.ToList(); // Not using ToList could be more performant
        if (list.Count == 1)
            list.Add(list[0].Copy()); // LineString needs at least two coordinates
        if (list.Count == 2)
            list.Add(list[0].Copy()); // LinearRing needs at least three coordinates
        if (!list.First().Equals2D(list.Last()))
            list.Add(list[0].Copy()); // LinearRing needs to be 'closed' (first should equal last)
        return new LinearRing(list.ToArray());
    }

    public static Polygon ToPolygon(this IEnumerable<Coordinate> coordinates, IEnumerable<IEnumerable<Coordinate>>? holes = null)
    {
        if (holes == null || holes.Count() == 0)
            return new Polygon(coordinates.ToLinearRing());
        return new Polygon(coordinates.ToLinearRing(), holes.Select(h => h.ToLinearRing()).ToArray());
    }
}
