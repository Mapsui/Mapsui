using System.Linq;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Extensions;

public static class LineStringExtensions
{
    public static LinearRing ToLinearRing(this LineString lineString)
    {
        var coordinates = lineString.Coordinates.ToList();
        // I think the LineString constructor does not accept lines of
        // length 0 or 1 so do not check for it.
        if (coordinates.Count == 2)
            coordinates.Add(coordinates[0].Copy());
        if (!coordinates.First().Equals2D(coordinates.Last()))
            coordinates.Add(coordinates[0].Copy());
        return new LinearRing(coordinates.ToArray());
    }
}
