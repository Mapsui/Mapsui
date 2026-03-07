using Mapsui.Layers;
using Mapsui.Rendering.Caching;
using Mapsui.Styles;
using System;
using System.Collections.Concurrent;
using System.Threading;

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
    /// The current render iteration. Incremented atomically by the map renderer after each
    /// render pass. Read by background threads during UpdateDrawables to stamp cache entries.
    /// Uses Interlocked for thread-safe access.
    /// </summary>
    private long _currentIteration;
    public long CurrentIteration
    {
        get => Interlocked.Read(ref _currentIteration);
        set => Interlocked.Exchange(ref _currentIteration, value);
    }

    /// <summary>Atomically increments CurrentIteration and returns the new value.</summary>
    public long IncrementIteration() => Interlocked.Increment(ref _currentIteration);

    /// <summary>
    /// Factory delegate for creating drawables from features and styles.
    /// Set by the MapRenderer during initialization when the Map is assigned to a MapControl.
    /// This enables the fetch pipeline to create drawables directly without needing
    /// access to renderer internals.
    /// </summary>
    public Func<Viewport, ILayer, IFeature, IStyle, RenderService, IDrawable?>? CreateDrawable { get; set; }

    public DrawableImageCache DrawableImageCache { get; }
    public VectorCache VectorCache { get; }
    /// <summary>
    /// Global tile cache. Kept for backward compatibility.
    /// </summary>
    public TileCache TileCache { get; }
    public ImageSourceCache ImageSourceCache { get; }

    // Opaque handle to a renderer-owned persistent render surface (e.g. an off-screen SKSurface).
    // Typed as object so that the core assembly has no dependency on SkiaSharp.
    // Managed exclusively through GetPersistentRenderSurface; disposed by Dispose().
    private object? _persistentRenderSurface;

    /// <summary>
    /// Returns the current persistent render surface, first passing it through <paramref name="ensure"/>.
    /// The delegate receives the current value (possibly null or stale), disposes it if needed,
    /// and returns the valid surface. Ownership stays with this <see cref="RenderService"/>.
    /// </summary>
    public object GetPersistentRenderSurface(Func<object?, object> ensure)
    {
        _persistentRenderSurface = ensure(_persistentRenderSurface);
        return _persistentRenderSurface;
    }

    /// <summary>
    /// Returns the drawable cache for the specified layer, or null if it has not been created yet.
    /// The cache is always created through <see cref="GetOrCreateLayerDrawableCache"/> so that
    /// the correct type is chosen by the renderer (e.g. <see cref="TileDrawableCache"/> vs
    /// <see cref="DrawableCache"/>). A silent <see cref="DrawableCache"/> fallback here would
    /// create the wrong type and cause tile-layer entries to be evicted unexpectedly.
    /// </summary>
    /// <param name="layerId">The unique identifier of the layer.</param>
    /// <returns>The cache, or null when the layer has not yet been through UpdateDrawables.</returns>
    public IDrawableCache? GetLayerDrawableCache(int layerId)
    {
        _layerDrawableCaches.TryGetValue(layerId, out var cache);
        return cache;
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
#pragma warning disable IDISP007 // _persistentRenderSurface is created and owned here; disposal is intentional
        (_persistentRenderSurface as IDisposable)?.Dispose();
#pragma warning restore IDISP007

        // Dispose per-layer caches
        foreach (var cache in _layerDrawableCaches.Values)
        {
            cache.Dispose();
        }
        _layerDrawableCaches.Clear();
    }
}
