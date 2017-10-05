using System;
using System.Collections.Concurrent;
using System.Threading;
using BruTile;

namespace Mapsui.Tests.Fetcher
{
    class CountingTileProvider : ITileProvider
    {
        readonly Random _random = new Random(32435);
        public ConcurrentDictionary<TileIndex, long> CountByTile { get; } = new ConcurrentDictionary<TileIndex, long>();
        public long TotalCount { get; private set; }

        public object syncRoot= new object();

        public byte[] GetTile(TileInfo tileInfo)
        {
            Thread.Sleep((int)(_random.NextDouble() * 10));
            CountByTile.AddOrUpdate(tileInfo.Index, 1, (index, count) => ++count);
            lock (syncRoot)
            {
                TotalCount++;
            }

            return new byte[0];
        }
    }
}
