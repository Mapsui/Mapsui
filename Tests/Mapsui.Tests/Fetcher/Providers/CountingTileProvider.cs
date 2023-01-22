using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BruTile;

namespace Mapsui.Tests.Fetcher.Providers;

internal class CountingTileProvider : ITileProvider
{
    private readonly Random _random = new Random(32435);
    public ConcurrentDictionary<TileIndex, long> CountByTile { get; } = new ConcurrentDictionary<TileIndex, long>();
    public long TotalCount;

    public virtual async Task<byte[]?> GetTileAsync(TileInfo tileInfo)
    {
        Thread.Sleep((int)(_random.NextDouble() * 10));

        CountByTile.AddOrUpdate(tileInfo.Index, 1, (index, count) => ++count);
        Interlocked.Increment(ref TotalCount);

        return await Task.FromResult(Array.Empty<byte>());
    }
}
