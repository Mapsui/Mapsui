using BruTile;
using SharpMap.Geometries;

namespace SharpMap.Geometries
{
    public static class ExtentExtensions
    {
        public static Extent ToExtent(this BoundingBox boundingBox)
        {
            return new Extent(boundingBox.MinX, boundingBox.MinY, boundingBox.MaxX, boundingBox.MaxY);
        }
    }
}

namespace BruTile
{
    public static class BoundingBoxExtensions
    {
        public static BoundingBox ToBoundingBox(this Extent extent)
        {
            return new BoundingBox(
                extent.MinX,
                extent.MinY,
                extent.MaxX,
                extent.MaxY);
        }
    }
}
