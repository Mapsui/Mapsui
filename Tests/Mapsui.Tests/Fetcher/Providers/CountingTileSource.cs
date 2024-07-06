using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BruTile;
using BruTile.Predefined;

namespace Mapsui.Tests.Fetcher.Providers;

internal class CountingTileSource : ILocalTileSource
{
    private readonly Random _random = new(32435);
    public ConcurrentDictionary<TileIndex, long> CountByTile { get; } = new ConcurrentDictionary<TileIndex, long>();
    public long TotalCount;

    public ITileSchema Schema { get; } = new GlobalSphericalMercator();

    public string Name { get; } = "TileSource";

    public Attribution Attribution { get; } = new Attribution();

    public virtual async Task<byte[]?> GetTileAsync(TileInfo tileInfo)
    {
        Thread.Sleep((int)(_random.NextDouble() * 10));

        CountByTile.AddOrUpdate(tileInfo.Index, 1, (index, count) => ++count);
        Interlocked.Increment(ref TotalCount);

        return await Task.FromResult(Array.Empty<byte>());
    }

}
