﻿using BruTile;

namespace Mapsui.Tests.Fetcher.Providers
{
    internal class NullTileProvider : CountingTileProvider
    {
        public override byte[]? GetTile(TileInfo tileInfo)
        {
            base.GetTile(tileInfo); // Just for counting

            return null;
        }
    }
}