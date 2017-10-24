using System;
using BruTile;

namespace Mapsui.Tests.Fetcher
{
    class FailingTileProvider : CountingTileProvider
    {
        public override byte[] GetTile(TileInfo tileInfo)
        {
            base.GetTile(tileInfo); // Just for counting
            throw new Exception("this provider sometimes fails");
        }
    }
}
