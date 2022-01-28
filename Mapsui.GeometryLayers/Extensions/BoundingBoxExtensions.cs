using System.Collections.Generic;
using Mapsui.Geometries;

namespace Mapsui.Extensions
{
    public static class BoundingBoxExtensions
    {
        public static IEnumerable<Point> AllVertices(this BoundingBox boundingBox)
        {
            return new[] { boundingBox.Min, boundingBox.Max };
        }

        public static BoundingBox Copy(this BoundingBox original)
        {
            return new BoundingBox(original.MinX, original.MinY, original.MaxX, original.MaxY);
        }

        public static MRect? ToMRect(this BoundingBox? boundingBox)
        {
            if (boundingBox == null) return null;
            return new MRect(boundingBox.MinX, boundingBox.MinY, boundingBox.MaxX, boundingBox.MaxY);
        }
    }
}