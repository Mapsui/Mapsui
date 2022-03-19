using BruTile;
using System.Threading.Tasks;

namespace Mapsui.Tests.Fetcher.Providers
{
    internal class NullTileProvider : CountingTileProvider
    {
        public override async Task<byte[]>? GetTileAsync(TileInfo tileInfo)
        {
            await base.GetTileAsync(tileInfo); // Just for counting

            return null;
        }
    }
}