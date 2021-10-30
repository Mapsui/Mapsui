using BruTile;
using Mapsui.Geometries;

namespace Mapsui.Extensions
{
    public static class MRectExtensions
    {
        public static BoundingBox? ToBoundingBox(this MRect boundingBox)
        {
            return new BoundingBox(boundingBox.MinX, boundingBox.MinY, boundingBox.MaxX, boundingBox.MaxY);
        }

        public static Extent ToExtent(this MRect rect)
        {
            return new Extent(rect.MinX, rect.MinY, rect.MaxX, rect.MaxY);
        }
    }
}
