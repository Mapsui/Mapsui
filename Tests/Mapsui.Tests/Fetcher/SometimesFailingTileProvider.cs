using System;
using BruTile;

namespace Mapsui.Tests.Fetcher
{
    class SometimesFailingTileProvider : ITileProvider
    {
        private readonly Random _random = new Random(DateTime.Now.Millisecond);
        
        public byte[] GetTile(TileInfo tileInfo)
        {
            if (_random.NextDouble() < 0.5)
            {
                if (_random.NextDouble() < 0.5)
                    throw new Exception("this provider sometimes fails");
                return null; // This means the tile is not available in the source
            }

            System.Threading.Thread.Sleep(10);
            return new byte[0];
        }
    }
}
