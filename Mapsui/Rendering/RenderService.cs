using Mapsui.Rendering.Caching;
using Mapsui.Styles;
using System;
using System.Collections.Concurrent;

namespace Mapsui.Rendering;

public sealed class RenderService : IDisposable
{
    private readonly ConcurrentDictionary<int, IDrawableCache> _layerDrawableCaches = new();

    public RenderService(int vectorCacheCapacity = 30000)
    {
        DrawableImageCache = new DrawableImageCache();
        TileCache = new TileCache();
        ImageSourceCache = new ImageSourceCache();
        VectorCache = new VectorCache(this, vectorCacheCapacity);
    }

    /// <summary>
    /// The current render iteration. Incremented by the map renderer after each render pass.
    /// Used by drawable caches to track which entries are still in use and which can be evicted.
    /// </summary>
    public long CurrentIteration { get; set; }

    public DrawableImageCache DrawableImageCache { get; }
    public VectorCache VectorCache { get; }
    /// <summary>
    /// Global tile cache. Kept for backward compatibility.
    /// </summary>
    public TileCache TileCache { get; }
    public ImageSourceCache ImageSourceCache { get; }

    /// <summary>
    /// Gets or creates a drawable cache for the specified layer.
    /// Each layer gets its own cache for pre-created drawable objects.
    /// Uses a <see cref="DrawableCache"/> by default.
    /// </summary>
    /// <param name="layerId">The unique identifier of the layer.</param>
    /// <returns>An IDrawableCache dedicated to the specified layer.</returns>
    public IDrawableCache GetLayerDrawableCache(int layerId)
    {
        return _layerDrawableCaches.GetOrAdd(layerId, _ => new DrawableCache());
    }

    /// <summary>
    /// Gets or creates a drawable cache for the specified layer using a custom factory.
    /// This allows renderers to specify their own cache type (e.g. <see cref="TileDrawableCache"/>).
    /// If a cache already exists for this layer, it is returned regardless of the factory.
    /// </summary>
    /// <param name="layerId">The unique identifier of the layer.</param>
    /// <param name="cacheFactory">Factory to create the cache if it doesn't exist yet.</param>
    /// <returns>An IDrawableCache dedicated to the specified layer.</returns>
    public IDrawableCache GetOrCreateLayerDrawableCache(int layerId, Func<IDrawableCache> cacheFactory)
    {
        return _layerDrawableCaches.GetOrAdd(layerId, _ => cacheFactory());
    }

    /// <summary>
    /// Checks whether a drawable cache exists for the specified layer.
    /// Used to detect layers that missed the DataChanged event and need
    /// their drawables created on the first render.
    /// </summary>
    /// <param name="layerId">The unique identifier of the layer.</param>
    /// <returns>True if a drawable cache already exists for this layer.</returns>
    public bool HasLayerDrawableCache(int layerId)
    {
        return _layerDrawableCaches.ContainsKey(layerId);
    }

    /// <summary>
    /// Cleans up all caches associated with a layer.
    /// Call this when a layer is removed from the map.
    /// </summary>
    /// <param name="layerId">The unique identifier of the layer.</param>
    public void CleanupLayerCaches(int layerId)
    {
        if (_layerDrawableCaches.TryRemove(layerId, out var drawableCache))
        {
            drawableCache.Dispose();
        }
    }

    public void Dispose()
    {
        DrawableImageCache.Dispose();
        VectorCache.Dispose();
        TileCache.Dispose();

        // Dispose per-layer caches
        foreach (var cache in _layerDrawableCaches.Values)
        {
            cache.Dispose();
        }
        _layerDrawableCaches.Clear();
    }
}
