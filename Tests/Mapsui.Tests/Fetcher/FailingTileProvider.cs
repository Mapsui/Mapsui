using System;
using BruTile;

namespace Mapsui.Tests.Fetcher
{
    public class FailingTileProvider : ITileProvider
    {
        public byte[] GetTile(TileInfo tileInfo)
        {
            throw new Exception("this provider always fails");
        }
    }
}
