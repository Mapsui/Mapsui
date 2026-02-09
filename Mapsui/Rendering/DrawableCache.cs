using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mapsui.Rendering;

/// <summary>
/// Caches drawable objects per feature (keyed by feature index within a layer).
/// Thread-safe: drawables are created on a background thread and read on the UI thread.
/// </summary>
public sealed class DrawableCache : IDisposable
{
    private readonly ConcurrentDictionary<long, IReadOnlyList<IDrawable>> _cache = new();

    /// <summary>
    /// Gets the drawables for a feature, or null if not cached.
    /// </summary>
    /// <param name="featureId">The feature identifier.</param>
    /// <returns>The cached drawables, or null if not found.</returns>
    public IReadOnlyList<IDrawable>? Get(long featureId)
    {
        return _cache.TryGetValue(featureId, out var drawables) ? drawables : null;
    }

    /// <summary>
    /// Stores drawables for a feature.
    /// </summary>
    /// <param name="featureId">The feature identifier.</param>
    /// <param name="drawables">The drawables to cache.</param>
    public void Set(long featureId, IReadOnlyList<IDrawable> drawables)
    {
        if (_cache.TryGetValue(featureId, out var existing))
        {
            DisposeDrawables(existing);
        }
        _cache[featureId] = drawables;
    }

    /// <summary>
    /// Clears all cached drawables and disposes them.
    /// </summary>
    public void Clear()
    {
        foreach (var entry in _cache.Values)
        {
            DisposeDrawables(entry);
        }
        _cache.Clear();
    }

    private static void DisposeDrawables(IReadOnlyList<IDrawable> drawables)
    {
        foreach (var drawable in drawables)
        {
            drawable.Dispose();
        }
    }

    public void Dispose()
    {
        Clear();
    }
}
