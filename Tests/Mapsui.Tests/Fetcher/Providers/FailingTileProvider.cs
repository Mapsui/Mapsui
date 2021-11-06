using System;
using BruTile;

namespace Mapsui.Tests.Fetcher.Providers
{
    internal class FailingTileProvider : CountingTileProvider
    {
        public override byte[] GetTile(TileInfo tileInfo)
        {
            base.GetTile(tileInfo); // Just for counting
            throw new Exception("this provider always fails");
        }
    }
}
