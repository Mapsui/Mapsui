using Mapsui.Geometries;

namespace Mapsui.Projection
{
    public interface IGeometryProjection : IProjection
    {
        void Project(string fromCRS, string toCRS, IGeometry geometry);
        void Project(string fromCRS, string toCRS, BoundingBox boundingBox);
    }
}