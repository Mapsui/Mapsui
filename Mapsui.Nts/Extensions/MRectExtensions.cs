using NetTopologySuite.Geometries;

namespace Mapsui.Extensions
{
    public static class MRectExtensions
    {
        //!!! todo: Rename to ToEnvelope after everything compiles.
        public static Envelope? ToBoundingBox(this MRect boundingBox)
        {
            return new Envelope(boundingBox.MinX, boundingBox.MinY, boundingBox.MaxX, boundingBox.MaxY);
        }

        public static Polygon ToPolygon(this MRect rect)
        {
            return new Polygon(new LinearRing(new[] {
                rect.TopLeft.ToCoordinate(),
                rect.TopRight.ToCoordinate(),
                rect.BottomRight.ToCoordinate(),
                rect.BottomLeft.ToCoordinate(),
                rect.TopLeft.ToCoordinate() }));
        }
    }
}
