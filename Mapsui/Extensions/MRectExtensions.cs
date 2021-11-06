using Mapsui.Geometries;

namespace Mapsui.Extensions
{
    public static class MRectExtensions
    {
        public static BoundingBox? ToBoundingBox(this MRect boundingBox)
        {
            return new BoundingBox(boundingBox.MinX, boundingBox.MinY, boundingBox.MaxX, boundingBox.MaxY);
        }

        public static Polygon ToPolygon(this MRect rect)
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
