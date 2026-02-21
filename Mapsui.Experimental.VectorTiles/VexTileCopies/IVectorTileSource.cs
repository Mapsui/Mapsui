using System.Threading.Tasks;
using VexTile.Common.Sources;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public interface IVectorTileSource : IBaseTileSource
{
    Task<VectorTile> GetVectorTileAsync(int x, int y, int zoom);

    Task<byte[]> GetTileAsync(int x, int y, int zoom);
}
