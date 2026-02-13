using System.Collections.Concurrent;

namespace Mapsui.Rendering;

/// <summary>
/// Caches drawable objects per feature (keyed by feature Id within a layer).
/// Thread-safe: drawables are created on a background thread and read on the UI thread.
/// Uses strict iteration-based eviction: anything not stamped with the current
/// iteration is removed on <see cref="Cleanup"/>.
/// </summary>
public sealed class DrawableCache : IDrawableCache
{
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
