using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Tiling;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class TileCache : IDisposable
{
    private const int _tilesToKeepMultiplier = 3;
    private const int _minimumTilesToKeep = 128; // in RasterStyle it was 32, I quadrupled it because now all tile Layers have one Cache
    private long _lastIteration;

    private readonly IDictionary<MRaster, TileCacheEntry> _tileCache =
        new ConcurrentDictionary<MRaster, TileCacheEntry>(new IdentityComparer<MRaster>());

    public TileCacheEntry GetOrCreate(MRaster raster, long currentIteration)
    {
        if (_tileCache.TryGetValue(raster, out var cachedTile))
        {
            // Get
            var entry = cachedTile;
            entry.IterationUsed = currentIteration;
            return entry;
        }
        else
        {
            // Create
            var entry = ToTileCacheEntry(raster.Data);
            _tileCache[raster] = entry;
            return _tileCache[raster];
        }
    }

    public void UpdateCache(long iteration)
    {
        if (iteration > 0 && _lastIteration != iteration)
        {
            _lastIteration = iteration;
            RemovedUnusedBitmapsFromCache();
        }
    }

    public static TileCacheEntry ToTileCacheEntry(byte[] data)
    {
        if (data.IsSkp())
        {
            return new TileCacheEntry(SKPicture.Deserialize(data));
        }

        using var skData = SKData.CreateCopy(data);
        var image = SKImage.FromEncodedData(skData);
        return new TileCacheEntry(image);
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

    private static void RemoveOldBitmaps(IDictionary<MRaster, TileCacheEntry> tileCache, int numberToRemove)
    {
        var counter = 0;
        var orderedKeys = tileCache.OrderBy(kvp => kvp.Value?.IterationUsed).Select(kvp => kvp.Key).ToList();
        foreach (var key in orderedKeys)
        {
            if (counter >= numberToRemove) break;
            var entry = tileCache[key];
            tileCache.Remove(key);
#pragma warning disable IDISP007
            entry.SKObject.Dispose();
#pragma warning restore IDISP007
            counter++;
        }
    }


    public void Dispose()
    {
        foreach (var key in _tileCache.Keys)
        {
            var tile = _tileCache[key];
            _tileCache.Remove(key); // Remove before dispose to make sure the disposed object is not used anymore
#pragma warning disable IDISP007
            tile.SKObject.Dispose();
#pragma warning restore IDISP007
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
