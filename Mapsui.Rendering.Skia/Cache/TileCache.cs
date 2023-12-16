using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class TileCache : ITileCache
{
    private const int _tilesToKeepMultiplier = 3;
    private const int _minimumTilesToKeep = 128; // in RasterStyle it was 32, I quadrupled it because now all tile Layers have one Cache
    private long _lastIteration;

    private readonly IDictionary<object, IBitmapInfo?> _tileCache =
        new ConcurrentDictionary<object, IBitmapInfo?>(new IdentityComparer<object>());

    public IBitmapInfo? GetOrCreate(MRaster raster, long currentIteration)
    {
        _tileCache.TryGetValue(raster, out var cachedBitmapInfo);
        var bitmapInfo = cachedBitmapInfo as BitmapInfo;
        if (BitmapHelper.InvalidBitmapInfo(bitmapInfo))
        {
            bitmapInfo = BitmapHelper.LoadBitmap(raster.Data);
            _tileCache[raster] = bitmapInfo;
        }

        if (BitmapHelper.InvalidBitmapInfo(bitmapInfo))
        {
            // remove invalid image from cache
            _tileCache.Remove(raster);
            return null;
        }

        bitmapInfo.IterationUsed = currentIteration;

        return bitmapInfo;
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

        if (tilesToRemove > 0) RemoveOldBitmaps(_tileCache, tilesToRemove);
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don\'t dispose injected")]
    private static void RemoveOldBitmaps(IDictionary<object, IBitmapInfo?> tileCache, int numberToRemove)
    {
        var counter = 0;
        var orderedKeys = tileCache.OrderBy(kvp => kvp.Value?.IterationUsed).Select(kvp => kvp.Key).ToList();
        foreach (var key in orderedKeys)
        {
            if (counter >= numberToRemove) break;
            var textureInfo = tileCache[key];
            tileCache.Remove(key);
            if (textureInfo is IDisposable textureInfoDisposable)
                textureInfoDisposable.Dispose();
            counter++;
        }
    }


    public void Dispose()
    {
        foreach (var bitmapInfo in _tileCache.Values)
        {
            bitmapInfo?.Dispose();
        }

        _tileCache.Clear();
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
