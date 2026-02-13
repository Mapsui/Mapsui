using System;
using System.Collections.Concurrent;
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
    public IDrawable? Get(long featureId, long iteration)
    {
        if (_cache.TryGetValue(featureId, out var entry))
        {
            entry.Iteration = iteration;
            return entry.Drawable;
        }
        return null;
    }

    /// <inheritdoc />
    public void Set(long featureId, IDrawable drawable, long iteration)
    {
        _cache.TryAdd(featureId, new CacheEntry(drawable, iteration));
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
#pragma warning disable IDISP007 // Don't dispose injected - cache owns these drawables
            entry.Drawable.Dispose();
#pragma warning restore IDISP007
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
#pragma warning disable IDISP007 // Don't dispose injected - cache owns these drawables
            entry.Drawable.Dispose();
#pragma warning restore IDISP007
            counter++;
        }
    }

    private sealed class CacheEntry(IDrawable drawable, long iteration)
    {
        public IDrawable Drawable { get; } = drawable;
        public long Iteration { get; set; } = iteration;
    }
}
