using System.Collections.Generic;
using Mapsui.Geometries;

namespace Mapsui.Projection
{
    public static class BoundingBoxIterator
    {
        public static IEnumerable<Point> AllVertices(this BoundingBox boundingBox)
        {
            return new[] { boundingBox.Min, boundingBox.Max };
        }

        public static IEnumerable<Point> GetCornerVertices(this BoundingBox boundingBox)
        {
            return new[] { boundingBox.BottomLeft, boundingBox.TopLeft, boundingBox.TopRight, boundingBox.BottomRight};
        }
    }
}
