using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Tiling;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class TileCache : ITileCache
{
    private const int _tilesToKeepMultiplier = 3;
    private const int _minimumTilesToKeep = 128; // in RasterStyle it was 32, I quadrupled it because now all tile Layers have one Cache
    private long _lastIteration;

    private readonly IDictionary<MRaster, IRenderedTile> _tileCache =
        new ConcurrentDictionary<MRaster, IRenderedTile>(new IdentityComparer<MRaster>());

    public IRenderedTile GetOrCreate(MRaster raster, long currentIteration)
    {
        if (_tileCache.TryGetValue(raster, out var cachedTile))
        {
            // Get
            var tile = cachedTile;
            tile.IterationUsed = currentIteration;
            return tile;
        }
        else
        {
            // Create
            var tile = ToRenderedTile(raster.Data);
            _tileCache[raster] = tile;
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

    public static IRenderedTile ToRenderedTile(byte[] data)
    {
        if (data.IsSkp())
        {
            return new PictureTile(SKPicture.Deserialize(data));
        }

        using var skData = SKData.CreateCopy(data);
        var image = SKImage.FromEncodedData(skData);
        return new ImageTile(image);
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

    private static void RemoveOldBitmaps(IDictionary<MRaster, IRenderedTile> tileCache, int numberToRemove)
    {
        var counter = 0;
        var orderedKeys = tileCache.OrderBy(kvp => kvp.Value?.IterationUsed).Select(kvp => kvp.Key).ToList();
        foreach (var key in orderedKeys)
        {
            if (counter >= numberToRemove) break;
            var tile = tileCache[key];
            tileCache.Remove(key);
            tile.DisposeIfDisposable();
            counter++;
        }
    }


    public void Dispose()
    {
        foreach (var key in _tileCache.Keys)
        {
            var tile = _tileCache[key];
            _tileCache.Remove(key); // Remove before dispose
            tile.Dispose();
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
