using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Rendering.Caching;

public sealed class TileCache : IDisposable
{
    private const int _tilesToKeepMultiplier = 3;
    private const int _minimumTilesToKeep = 256; // in RasterStyle it was 32, I quadrupled it because now all tile Layers have one Cache
    private long _lastIteration;

    private readonly ConcurrentDictionary<MRaster, ITileCacheEntry> _tileCache = new(new IdentityComparer<MRaster>());

    public ITileCacheEntry GetOrAdd(MRaster raster, Func<MRaster, ITileCacheEntry> create, long currentIteration)
    {
        var entry = _tileCache.GetOrAdd(raster, create);
        entry.IterationUsed = currentIteration;
        return entry;
    }

    public void UpdateCache(long iteration)
    {
        if (iteration > 0 && _lastIteration != iteration)
        {
            _lastIteration = iteration;
            RemovedUnusedBitmapsFromCache();
        }
    }

    private void RemovedUnusedBitmapsFromCache()
    {
        var tilesUsedInCurrentIteration =
            _tileCache.Values.Count(i => i?.IterationUsed == _lastIteration);
        var tilesToKeep = tilesUsedInCurrentIteration * _tilesToKeepMultiplier;
        tilesToKeep = Math.Max(tilesToKeep, _minimumTilesToKeep);
        var tilesToRemove = _tileCache.Keys.Count - tilesToKeep;

        if (tilesToRemove > 0)
            RemoveOldBitmaps(_tileCache, tilesToRemove);
    }

    private static void RemoveOldBitmaps(IDictionary<MRaster, ITileCacheEntry> tileCache, int numberToRemove)
    {
        var counter = 0;
        var orderedKeys = tileCache.OrderBy(kvp => kvp.Value?.IterationUsed).Select(kvp => kvp.Key).ToList();
        foreach (var key in orderedKeys)
        {
            if (counter >= numberToRemove) break;
            var entry = tileCache[key];
            _ = tileCache.Remove(key);
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
            if (_tileCache.TryRemove(key, out var cachedTile)) // Remove before dispose to make sure the disposed object is not used anymore
            {
#pragma warning disable IDISP007
                if (cachedTile.Data is IDisposable disposable)
                    disposable.Dispose();
#pragma warning restore IDISP007
            }
        }
    }
}

public class IdentityComparer<T> : IEqualityComparer<T> where T : class
{
    public bool Equals(T? obj, T? otherObj)
    {
        return obj == otherObj;
    }

    public int GetHashCode(T obj)
    {
        return obj.GetHashCode();
    }
}
