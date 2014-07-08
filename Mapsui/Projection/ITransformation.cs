using Mapsui.Geometries;

namespace Mapsui.Projection
{
    public interface ITransformation
    {
        IGeometry Transform(string fromCRS, string toCRS, IGeometry geometry);
        BoundingBox Transform(string fromCRS, string toCRS, BoundingBox boundingBox);
    }
}