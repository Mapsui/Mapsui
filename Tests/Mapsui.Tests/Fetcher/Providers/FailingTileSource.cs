using System;
using System.Threading.Tasks;
using BruTile;

namespace Mapsui.Tests.Fetcher.Providers;

internal class FailingTileSource : CountingTileSource
{
    public override async Task<byte[]?> GetTileAsync(TileInfo tileInfo)
    {
        _ = await base.GetTileAsync(tileInfo)!; // Just for counting
        throw new Exception("this provider always fails");
    }
}
