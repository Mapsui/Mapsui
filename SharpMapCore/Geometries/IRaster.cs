namespace SharpMap.Geometries
{
    public interface IRaster : IGeometry
    {
        byte[] Data { get; }
        BoundingBox GetBoundingBox();
    }
}
