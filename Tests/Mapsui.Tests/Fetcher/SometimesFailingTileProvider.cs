using System;
using System.Linq;
using BruTile;

namespace Mapsui.Tests.Fetcher
{
    class SometimesFailingTileProvider : ITileProvider
    {
        private readonly double _probabilityFail = 0.5;
        private readonly Random _random = new Random(DateTime.Now.Millisecond);

        public SometimesFailingTileProvider(double probabilityFail = 0.5)
        {
            _probabilityFail = probabilityFail;
        }

        public byte[] GetTile(TileInfo tileInfo)
        {
            if (_random.NextDouble() < _probabilityFail) throw new Exception("this provider always fails");

            return new byte[0];
        }
    }
}
