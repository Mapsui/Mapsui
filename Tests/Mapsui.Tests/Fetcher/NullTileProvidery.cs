using BruTile;

namespace Mapsui.Tests.Fetcher
{
    public class NullTileProvider : ITileProvider
    {
        public byte[] GetTile(TileInfo tileInfo)
        {
            return null;
        }
    }
}