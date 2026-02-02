using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Rendering.Caching;

/// <summary>
/// Cache for rendered tile images, keyed by feature Id.
/// Each feature has a unique Id, making it a natural cache key.
/// </summary>
public sealed class FeatureIdTileCache : IDisposable
{
    private const int _tilesToKeepMultiplier = 3;
    private const int _minimumTilesToKeep = 256;
    private long _lastIteration;

    private readonly ConcurrentDictionary<long, ITileCacheEntry> _tileCache = new();

    public ITileCacheEntry GetOrAdd(long featureId, Func<long, ITileCacheEntry> create, long currentIteration)
    {
        var entry = _tileCache.GetOrAdd(featureId, create);
        entry.IterationUsed = currentIteration;
        return entry;
    }

    public void UpdateCache(long iteration)
    {
        if (iteration > 0 && _lastIteration != iteration)
        {
            _lastIteration = iteration;
            RemoveUnusedEntriesFromCache();
        }
    }

    private void RemoveUnusedEntriesFromCache()
    {
        var tilesUsedInCurrentIteration =
            _tileCache.Values.Count(i => i?.IterationUsed == _lastIteration);
        var tilesToKeep = tilesUsedInCurrentIteration * _tilesToKeepMultiplier;
        tilesToKeep = Math.Max(tilesToKeep, _minimumTilesToKeep);
        var tilesToRemove = _tileCache.Keys.Count - tilesToKeep;

        if (tilesToRemove > 0)
            RemoveOldEntries(_tileCache, tilesToRemove);
    }

    private static void RemoveOldEntries(IDictionary<long, ITileCacheEntry> cache, int numberToRemove)
    {
        var counter = 0;
        var orderedKeys = cache.OrderBy(kvp => kvp.Value?.IterationUsed).Select(kvp => kvp.Key).ToList();
        foreach (var key in orderedKeys)
        {
            if (counter >= numberToRemove) break;
            var entry = cache[key];
            _ = cache.Remove(key);
#pragma warning disable IDISP007
            if (entry.Data is IDisposable disposable)
                disposable.Dispose();
#pragma warning restore IDISP007
            counter++;
        }
    }

    public void Dispose()
    {
        foreach (var key in _tileCache.Keys)
        {
            if (_tileCache.TryRemove(key, out var cachedTile))
            {
#pragma warning disable IDISP007
                if (cachedTile.Data is IDisposable disposable)
                    disposable.Dispose();
#pragma warning restore IDISP007
            }
        }
    }
}
