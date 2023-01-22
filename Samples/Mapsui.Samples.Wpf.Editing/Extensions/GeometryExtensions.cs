using System;
using Mapsui.Samples.Wpf.Editing;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Extensions;

public static class GeometryExtensions
{
    public static Geometry? InsertCoordinate(this Geometry? geometry, Coordinate coordinate, int segment)
    {
        if (geometry is null)
            return null;

        var vertices = geometry.MainCoordinates();
        vertices.Insert(segment + 1, coordinate);

        if (geometry is Polygon)
            return vertices.ToPolygon();
        else if (geometry is LineString)
            return vertices.ToLineString();
        else
            throw new NotSupportedException();
    }

    public static Geometry? DeleteCoordinate(this Geometry? geometry, int index)
    {
        if (geometry is null)
            return null;

        var vertices = geometry.MainCoordinates();
        vertices.RemoveAt(index);

        if (geometry is Polygon)
            return vertices.ToPolygon();
        else if (geometry is LineString)
            return vertices.ToLineString();
        else
            throw new NotSupportedException();
    }
}
