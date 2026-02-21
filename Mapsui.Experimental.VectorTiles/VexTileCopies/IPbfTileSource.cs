using System.Threading.Tasks;
using VexTile.Common.Sources;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public interface IPbfTileSource : IBaseTileSource
{
    Task<VectorTile?> GetTileAsync();
}
