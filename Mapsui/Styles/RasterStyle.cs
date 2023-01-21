using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mapsui.Rendering;

namespace Mapsui.Styles;

public class RasterStyle : IStyle
{
    private const int TilesToKeepMultiplier = 3;
    private const int MinimumTilesToKeep = 32;
    private long _lastIteration;
    private readonly IDictionary<object, IBitmapInfo?> _tileCache =
        new Dictionary<object, IBitmapInfo?>(new IdentityComparer<object>());

    public double MinVisible { get; set; } = 0;
    public double MaxVisible { get; set; } = double.MaxValue;
    public bool Enabled { get; set; } = true;
    public float Opacity { get; set; } = 1.0f;

    public IDictionary<object, IBitmapInfo?> TileCache => _tileCache;

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
        var tilesToKeep = tilesUsedInCurrentIteration * TilesToKeepMultiplier;
        tilesToKeep = Math.Max(tilesToKeep, MinimumTilesToKeep);
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
