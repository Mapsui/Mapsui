﻿using BruTile;
using System.Threading.Tasks;

namespace Mapsui.Tests.Fetcher.Providers;

internal class NullTileSource : CountingTileSource
{
    public override async Task<byte[]?> GetTileAsync(TileInfo tileInfo)
    {
        _ = await base.GetTileAsync(tileInfo); // Just for counting

        return null;
    }
}
