using System.Collections.Generic;
using Mapsui.Nts.Extensions;
using NetTopologySuite.Geometries;
using Mapsui.Utilities;

namespace Mapsui.Samples.Wpf.Editing.Editing;

public static class EditHelper
{
    /// <summary>
    /// Determines if a coordinate should be inserted into list of coordinates.        /// 
    /// It fails if the distance of the location to the line is larger than the screenDistance.
    /// </summary>
    /// <param name="worldPosition">The position that should perhaps be inserted.</param>
    /// <param name="resolution">The map resolution that is needed to calculte the distance.</param>
    /// <param name="coordinates">The coordinates to insert into.</param>
    /// <param name="screenDistance">The allowed screen distance. This value is determined by the
    /// size of a symbol or the line width.</param>
    /// <param name="segment">The segment to insert it at.</param>
    /// <returns></returns>
    public static bool ShouldInsert(MPoint worldPosition, double resolution, List<Coordinate> coordinates, double screenDistance, out int segment)
    {
        (var distance, segment) = GetDistanceAndSegment(worldPosition, coordinates);
        return IsCloseEnough(distance, resolution, screenDistance);
    }

    private static bool IsCloseEnough(double distance, double resolution, double screenDistance)
    {
        return distance <= resolution * screenDistance;
    }

    private static (double Distance, int segment) GetDistanceAndSegment(MPoint point, IList<Coordinate> points)
    {
        // Move this to Mapsui

        var minDist = double.MaxValue;
        var segment = 0;

        for (var i = 0; i < points.Count - 1; i++)
        {
            var dist = Algorithms.DistancePointLine(point, points[i].ToMPoint(), points[i + 1].ToMPoint());
            if (dist < minDist)
            {
                minDist = dist;
                segment = i;
            }
        }

        return (minDist, segment);
    }
}
