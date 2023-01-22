using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Extensions;

public static class GeometryExtensions
{
    public static IEnumerable<GeometryFeature> ToFeatures(this IEnumerable<Geometry> geometries)
    {
        return geometries.Select(g => new GeometryFeature(g)).ToList();
    }

    public static GeometryFeature ToFeature(this Geometry geometry)
    {
        return new GeometryFeature(geometry);
    }
}
