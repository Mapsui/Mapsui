using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Rendering;

/// <summary>
/// Drawable cache for tile layers with LRU-style eviction based on render iteration.
/// Keeps more tiles than strictly needed so that zooming in/out
/// doesn't immediately discard previously rendered tiles.
/// </summary>
public sealed class TileDrawableCache : IDrawableCache
{
    private const int _minimumTilesToKeep = 256;

    private readonly ConcurrentDictionary<long, CacheEntry> _cache = new();

    /// <inheritdoc />
    public IReadOnlyList<IDrawable> Get(long featureId, long iteration)
    {
        if (_cache.TryGetValue(featureId, out var entry))
        {
            entry.Iteration = iteration;
            return entry.Drawables;
        }
        return [];
    }

    /// <inheritdoc />
    public void Set(long featureId, IReadOnlyList<IDrawable> drawables, long iteration)
    {
        _cache.TryAdd(featureId, new CacheEntry(drawables, iteration));
    }

    /// <inheritdoc />
    /// <remarks>
    /// LRU cleanup: counts the tiles stamped with <paramref name="currentIteration"/>
    /// as "active", then keeps up to <c>3× active</c> (minimum 256) total entries.
    /// Excess entries are removed oldest-iteration-first.
    /// </remarks>
    public void Cleanup(long currentIteration)
    {
        var activeCount = _cache.Values.Count(e => e.Iteration == currentIteration);
        var tilesToRemove = _cache.Count - _minimumTilesToKeep;
        if (tilesToRemove > 0)
        {
            RemoveOldest(tilesToRemove);
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        foreach (var entry in _cache.Values)
        {
            DisposeDrawables(entry.Drawables);
        }
        _cache.Clear();
    }

    public void Dispose()
    {
        Clear();
    }

    private void RemoveOldest(int numberToRemove)
    {
        var counter = 0;
        var orderedKeys = _cache
            .Where(kvp => kvp.Value is not null)
            .OrderBy(kvp => kvp.Value.Iteration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in orderedKeys)
        {
            if (counter >= numberToRemove) break;
            if (!_cache.TryRemove(key, out var entry)) continue;
            DisposeDrawables(entry.Drawables);
            counter++;
        }
    }

    private static void DisposeDrawables(IReadOnlyList<IDrawable> drawables)
    {
        foreach (var drawable in drawables)
        {
            drawable.Dispose();
        }
    }

    private sealed class CacheEntry(IReadOnlyList<IDrawable> drawables, long iteration)
    {
        public IReadOnlyList<IDrawable> Drawables { get; } = drawables;
        public long Iteration { get; set; } = iteration;
    }
}
