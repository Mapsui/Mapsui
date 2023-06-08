using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Extensions;

public static partial class GeometryExtensions
{
    public static IEnumerable<GeometryFeature> ToFeatures(this IEnumerable<Geometry> geometries)
    {
        return geometries.Select(g => new GeometryFeature(g)).ToList();
    }

    public static GeometryFeature ToFeature(this Geometry geometry)
    {
        return new GeometryFeature(geometry);
    }

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
