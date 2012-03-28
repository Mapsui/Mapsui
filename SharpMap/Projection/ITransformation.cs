using SharpMap.Geometries;

namespace SharpMap.Projection
{
    public interface ITransformation
    {
        Geometry Transform(int layerSRID, Geometry geometry);
        Geometry ReverseTransform(int layerSRID, Geometry geometry);
        BoundingBox Transform(int layerSRID, BoundingBox boundingBox);
        BoundingBox ReverseTransform(int layerSRID, BoundingBox boundingBox);
    }
}