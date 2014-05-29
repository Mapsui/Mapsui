using Mapsui.Geometries;

namespace Mapsui.Projection
{
    public interface ITransformation
    {
        IGeometry Transform(int fromSRID, int toSRID, IGeometry geometry);
        BoundingBox Transform(int fromSRID, int toSRID, BoundingBox boundingBox);
    }
}