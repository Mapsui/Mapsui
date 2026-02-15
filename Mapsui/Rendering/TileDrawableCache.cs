using Mapsui.Logging;
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
    private const int _minimumTilesToKeep = 64;

    private readonly ConcurrentDictionary<DrawableCacheKey, CacheEntry> _cache = new();

    /// <inheritdoc />
    public IDrawable? Get(DrawableCacheKey key, long iteration)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            entry.Iteration = iteration;
            return entry.Drawable;
        }
        return null;
    }

    /// <inheritdoc />
    public void Set(DrawableCacheKey key, IDrawable drawable, long iteration)
    {
        if (!_cache.TryAdd(key, new CacheEntry(drawable, iteration)))
        {
            Logger.Log(LogLevel.Warning, $"Drawable for key ({key.FeatureGenerationId}, {key.StyleGenerationId}) already exists in cache. This may indicate a race condition between DataChanged and Render threads.");
            // Key already exists (race between DataChanged and Render threads).
            // Dispose the new drawable to avoid leaking native resources (e.g. SKImage).
#pragma warning disable IDISP007 // Don't dispose injected - we own this drawable, cache rejected it
            drawable.Dispose();
#pragma warning restore IDISP007
        }
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
