using Mapsui.Logging;
using System.Collections.Concurrent;

namespace Mapsui.Rendering;

/// <summary>
/// Caches drawable objects per (feature, style) combination.
/// Thread-safe: drawables are created on a background thread and read on the UI thread.
/// Uses strict iteration-based eviction: anything not stamped with the current
/// iteration is removed on <see cref="Cleanup"/>.
/// </summary>
public sealed class DrawableCache : IDrawableCache
{
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
            Logger.Log(LogLevel.Warning, $"DrawableCache: Failed to add drawable for key ({key.FeatureGenerationId}, {key.StyleGenerationId}) (iteration {iteration}). Key already exists.");
            // Key already exists (race between DataChanged and Render threads).
            // Dispose the new drawable to avoid leaking native resources (e.g. SKImage).
#pragma warning disable IDISP007 // Don't dispose injected - we own this drawable, cache rejected it
            drawable.Dispose();
#pragma warning restore IDISP007
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Strict cleanup: removes and disposes every entry whose iteration is not
    /// <paramref name="currentIteration"/>. This ensures that features no longer
    /// in the viewport are cleaned up immediately.
    /// </remarks>
    public void Cleanup(long currentIteration)
    {
        foreach (var key in _cache.Keys)
        {
            if (_cache.TryGetValue(key, out var entry) && entry.Iteration != currentIteration)
            {
                if (_cache.TryRemove(key, out var removed))
                {
#pragma warning disable IDISP007 // Don't dispose injected - cache owns these drawables
                    removed.Drawable.Dispose();
#pragma warning restore IDISP007
                }
            }
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

    private sealed class CacheEntry(IDrawable drawable, long iteration)
    {
        public IDrawable Drawable { get; } = drawable;
        public long Iteration { get; set; } = iteration;
    }
}
