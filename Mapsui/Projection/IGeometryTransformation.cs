using Mapsui.Geometries;

namespace Mapsui.Projection
{
    public interface IGeometryTransformation : ITransformation
    {
        void Transform(string fromCRS, string toCRS, IGeometry geometry);
        void Transform(string fromCRS, string toCRS, BoundingBox boundingBox);
    }
}