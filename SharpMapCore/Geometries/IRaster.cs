namespace SharpMap.Geometries
{
    public interface IRaster : IGeometry
    {
        byte[] Data { get; }
        new BoundingBox GetBoundingBox();
    }
}
