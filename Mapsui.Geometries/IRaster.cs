using System.IO;
namespace Mapsui.Geometries
{
    public interface IRaster : IGeometry
    {
        MemoryStream Data { get; }
        new BoundingBox GetBoundingBox();
        long TickFetched { get; }
    }
}
