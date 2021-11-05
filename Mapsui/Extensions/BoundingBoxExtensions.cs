using BruTile;
using Mapsui.Geometries;

namespace Mapsui.Extensions
{
    public static class BoundingBoxExtensions
    {
        public static BoundingBox Copy(this BoundingBox original)
        {
            return new BoundingBox(original.MinX, original.MinY, original.MaxX, original.MaxY);
        }

        public static bool IsInitialized(this BoundingBox? box)
        {
            if (box == null) return false;
            if (double.IsNaN(box.Width)) return false;
            if (double.IsNaN(box.Height)) return false;
            if (box.Centroid == null) return false;

            return true;
        }

        public static Extent ToExtent(this BoundingBox? boundingBox)
        {
            return new Extent(boundingBox.MinX, boundingBox.MinY, boundingBox.MaxX, boundingBox.MaxY);
        }

        public static MRectangle? ToMRect(this BoundingBox? boundingBox)
        {
            if (boundingBox == null) return null;
            return new MRectangle(boundingBox.MinX, boundingBox.MinY, boundingBox.MaxX, boundingBox.MaxY);
        }
    }
}