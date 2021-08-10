using System.IO;

namespace Mapsui.Geometries
{
    public interface IRaster : IGeometry
    {
        MemoryStream Data { get; }
        long TickFetched { get; }
        new BoundingBox BoundingBox { get; }
    }
}