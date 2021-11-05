using System.IO;

namespace Mapsui.Geometries
{
    public interface IRaster 
    {
        MemoryStream Data { get; }
        long TickFetched { get; }
    }
}