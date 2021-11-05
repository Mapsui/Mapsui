using BruTile;
using Mapsui.Geometries;

namespace Mapsui.Extensions
{
    public static class MRectExtensions
    {
        public static BoundingBox? ToBoundingBox(this MRectangle boundingBox)
        {
            return new BoundingBox(boundingBox.MinX, boundingBox.MinY, boundingBox.MaxX, boundingBox.MaxY);
        }

        public static Extent ToExtent(this MRectangle rect)
        {
            return new Extent(rect.MinX, rect.MinY, rect.MaxX, rect.MaxY);
        }

        public static Polygon ToPolygon(this MRectangle rect)
        {
            return new Polygon(new LinearRing(new[] {
                rect.TopLeft.ToPoint(),
                rect.TopRight.ToPoint(),
                rect.BottomRight.ToPoint(),
                rect.BottomLeft.ToPoint(),
                rect.TopLeft.ToPoint() }));
        }
    }
}
