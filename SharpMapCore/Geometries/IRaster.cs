using System.IO;
namespace SharpMap.Geometries
{
    public interface IRaster : IGeometry
    {
        MemoryStream Data { get; }
        new BoundingBox GetBoundingBox();
        long TickFetched { get; }
    }
}
