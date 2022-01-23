using Mapsui.Geometries;

namespace Mapsui.Extensions
{
    public static class BoundingBoxExtensions
    {
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