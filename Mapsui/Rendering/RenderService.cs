using Mapsui.Rendering.Caching;
using Mapsui.Styles;
using System;
using System.Collections.Concurrent;

namespace Mapsui.Rendering;

public sealed class RenderService : IDisposable
{
    private readonly ConcurrentDictionary<int, FeatureIdTileCache> _layerFeatureIdTileCaches = new();
    private readonly ConcurrentDictionary<int, DrawableCache> _layerDrawableCaches = new();

    public RenderService(int vectorCacheCapacity = 30000)
    {
        DrawableImageCache = new DrawableImageCache();
        TileCache = new TileCache();
        ImageSourceCache = new ImageSourceCache();
        VectorCache = new VectorCache(this, vectorCacheCapacity);
    }

    public DrawableImageCache DrawableImageCache { get; }
    public VectorCache VectorCache { get; }
    /// <summary>
    /// Global tile cache. Kept for backward compatibility.
    /// </summary>
    public TileCache TileCache { get; }
    public ImageSourceCache ImageSourceCache { get; }

    /// <summary>
    /// Gets or creates a FeatureIdTileCache for the specified layer.
    /// Each layer gets its own cache to avoid competition for cache space.
    /// Uses feature Id as cache key.
    /// </summary>
    /// <param name="layerId">The unique identifier of the layer.</param>
    /// <returns>A FeatureIdTileCache dedicated to the specified layer.</returns>
    public FeatureIdTileCache GetLayerFeatureIdTileCache(int layerId)
    {
        return _layerFeatureIdTileCaches.GetOrAdd(layerId, _ => new FeatureIdTileCache());
    }

    /// <summary>
    /// Gets or creates a DrawableCache for the specified layer.
    /// Each layer gets its own cache for pre-created drawable objects.
    /// </summary>
    /// <param name="layerId">The unique identifier of the layer.</param>
    /// <returns>A DrawableCache dedicated to the specified layer.</returns>
    public DrawableCache GetLayerDrawableCache(int layerId)
    {
        return _layerDrawableCaches.GetOrAdd(layerId, _ => new DrawableCache());
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
        if (_layerFeatureIdTileCaches.TryRemove(layerId, out var featureIdTileCache))
        {
            featureIdTileCache.Dispose();
        }

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
        foreach (var cache in _layerFeatureIdTileCaches.Values)
        {
            cache.Dispose();
        }
        _layerFeatureIdTileCaches.Clear();

        foreach (var cache in _layerDrawableCaches.Values)
        {
            cache.Dispose();
        }
        _layerDrawableCaches.Clear();
    }
}
