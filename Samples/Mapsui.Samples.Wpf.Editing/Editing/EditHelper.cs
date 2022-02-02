using System.Collections.Generic;
using Mapsui.Nts.Extensions;
using NetTopologySuite.Geometries;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Wpf.Editing.Editing
{
    public static class EditHelper
    {
        /// <summary>
        /// Inserts a vertex into a list of vertices at the location of the MapInfo location.
        /// It fails if the distance of the location to the line is larger than the screenDistance.
        /// </summary>
        /// <param name="mapInfo">The MapInfo object that contains the location</param>
        /// <param name="vertices">The list of vertices to insert into</param>
        /// <param name="screenDistance"></param>
        /// <returns></returns>
        public static bool TryInsertVertex(MapInfo mapInfo, IList<Coordinate> vertices, double screenDistance)
        {
            if (mapInfo.WorldPosition is null)
                return false;

            var (distance, segment) = GetDistanceAndSegment(mapInfo.WorldPosition, vertices);
            if (IsCloseEnough(distance, mapInfo.Resolution, screenDistance))
            {
                vertices.Insert(segment + 1, mapInfo.WorldPosition.ToCoordinate());
                return true;
            }
            return false;
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
}
