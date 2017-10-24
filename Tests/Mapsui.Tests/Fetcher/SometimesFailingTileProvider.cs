using System;
using BruTile;

namespace Mapsui.Tests.Fetcher
{
    class SometimesFailingTileProvider : CountingTileProvider
    {
        private readonly Random _random = new Random(DateTime.Now.Millisecond);
        
        public override byte[] GetTile(TileInfo tileInfo)
        {
            base.GetTile(tileInfo); // Just for counting

            if (_random.NextDouble() < 0.5)
            {
                if (_random.NextDouble() < 0.5)
                    throw new Exception("this provider sometimes fails");
                return null; // This means the tile is not available in the source
            }
            return new byte[0];
        }
    }
}
