using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Extensions;

public static class MRectExtensions
{
    public static Envelope ToEnvelope(this MRect rect)
    {
        return new Envelope(rect.MinX, rect.MaxX, rect.MinY, rect.MaxY);
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
