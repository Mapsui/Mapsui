using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    private readonly IDictionary<MRaster, IRenderedTile?> _tileCache =
        new ConcurrentDictionary<MRaster, IRenderedTile?>(new IdentityComparer<MRaster>());

    public IRenderedTile? GetOrCreate(MRaster raster, long currentIteration)
    {
        if (_tileCache.TryGetValue(raster, out var cachedBitmapInfo))
        {
            var bitmapInfo = cachedBitmapInfo;
            if (!IsValid(bitmapInfo))
            {
                bitmapInfo = ToRenderedTile(raster.Data);
                _tileCache[raster] = bitmapInfo;
            }

            if (!IsValid(bitmapInfo))
            {
                // Remove invalid image from cache
                _tileCache.Remove(raster);
                return null;
            }

            bitmapInfo.IterationUsed = currentIteration;

            return bitmapInfo;
        }
        else // Here we need to Create
        {
            var bitmapInfo = ToRenderedTile(raster.Data);
            _tileCache[raster] = bitmapInfo;
            return _tileCache[raster];
        }
    }

    public static bool IsValid([NotNullWhen(true)] IRenderedTile? bitmapInfo)
    {
        return bitmapInfo is not null;
    }

    public void UpdateCache(long iteration)
    {
        if (iteration > 0 && _lastIteration != iteration)
        {
            _lastIteration = iteration;
            RemovedUnusedBitmapsFromCache();
        }
    }

    public static IRenderedTile? ToRenderedTile(byte[] data)
    {
        if (data.IsSKPicture())
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

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don\'t dispose injected")]
    private static void RemoveOldBitmaps(IDictionary<MRaster, IRenderedTile?> tileCache, int numberToRemove)
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
