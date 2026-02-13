using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Experimental.Rendering.Skia;

/// <summary>
/// Static orchestrator that manages background preparation and caching of drawables.
/// All cache interaction (Get, Set, Cleanup) happens here — not inside the renderers.
/// Drawable caches are stored in <see cref="RenderService"/> for centralized management.
/// </summary>
public static class DrawableRenderer
{
    /// <summary>
    /// Updates drawables for a layer on a background thread. For each feature that is not
    /// yet cached, calls <see cref="ITwoStepStyleRenderer.CreateDrawable"/> and stores
    /// the result in the cache. Then calls <see cref="IDrawableCache.Cleanup"/> to evict
    /// stale entries based on the current render iteration.
    /// </summary>
    public static void UpdateDrawables(Viewport viewport, ILayer layer,
        IReadOnlyDictionary<Type, IStyleRenderer> styleRenderers, RenderService renderService)
    {
        if (!styleRenderers.Values.Any(r => r is ITwoStepStyleRenderer)) return;

        try
        {
            var extent = viewport.ToExtent();
            if (extent is null) return;

            var features = layer.GetFeatures(extent, viewport.Resolution).ToList();

            // Ensure the cache exists for this layer.
            EnsureDrawableCache(layer, styleRenderers, renderService);

#pragma warning disable IDISP001 // Dispose created - cache managed by RenderService
            var cache = renderService.GetLayerDrawableCache(layer.Id);
#pragma warning restore IDISP001

            var currentIteration = renderService.CurrentIteration;

            // Create drawables only for features not yet in the cache.
            // Get stamps existing entries with the current iteration so
            // Cleanup knows they are still in use.
            foreach (var feature in features)
            {
                if (cache.Get(feature.Id, currentIteration) is not null)
                    continue; // Already cached and stamped, skip.

#pragma warning disable IDISP001 // Dispose created - ownership transfers to cache via Set
                var drawable = CreateDrawableForFeature(viewport, layer, feature, styleRenderers, renderService);
#pragma warning restore IDISP001
                if (drawable is not null)
                {
                    cache.Set(feature.Id, drawable, currentIteration);
                }
            }

            // Evict stale entries. The cache implementation determines the strategy
            // (e.g. TileDrawableCache uses LRU, DrawableCache uses strict iteration-based).
            cache.Cleanup(currentIteration);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"Error updating drawables for layer '{layer.Name}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tries to get a cached drawable for a feature. Called from the render thread.
    /// Stamps the cache entry with <paramref name="iteration"/> so that Cleanup
    /// knows the entry was recently used.
    /// </summary>
    public static IDrawable? TryGetDrawable(RenderService renderService, int layerId, long featureId, long iteration)
    {
#pragma warning disable IDISP001, IDISP004 // Cache managed by RenderService
        return renderService.GetLayerDrawableCache(layerId).Get(featureId, iteration);
#pragma warning restore IDISP001, IDISP004
    }

    private static void EnsureDrawableCache(ILayer layer,
        IReadOnlyDictionary<Type, IStyleRenderer> styleRenderers, RenderService renderService)
    {
        if (renderService.HasLayerDrawableCache(layer.Id)) return;

        var twoStep = FindTwoStepRenderer(layer, styleRenderers);
        if (twoStep is not null)
        {
#pragma warning disable IDISP004 // Don't ignore created IDisposable - cache managed by RenderService
            renderService.GetOrCreateLayerDrawableCache(layer.Id, twoStep.CreateCache);
#pragma warning restore IDISP004
        }
    }

    private static ITwoStepStyleRenderer? FindTwoStepRenderer(ILayer layer,
        IReadOnlyDictionary<Type, IStyleRenderer> styleRenderers)
    {
        if (layer.Style is not null)
        {
            var styleType = layer.Style.GetType();
            if (styleRenderers.TryGetValue(styleType, out var renderer) && renderer is ITwoStepStyleRenderer p)
                return p;
        }

        foreach (var renderer in styleRenderers.Values)
        {
            if (renderer is ITwoStepStyleRenderer twoStep)
                return twoStep;
        }
        return null;
    }

    /// <summary>
    /// Calls <see cref="ITwoStepStyleRenderer.CreateDrawable"/> for each applicable style
    /// on the feature. Returns the combined drawable, or null if none were created.
    /// </summary>
#pragma warning disable IDISP015 // Member should not return created and cached instance - ownership transfers to caller
    private static IDrawable? CreateDrawableForFeature(Viewport viewport, ILayer layer,
        IFeature feature, IReadOnlyDictionary<Type, IStyleRenderer> styleRenderers, RenderService renderService)
#pragma warning restore IDISP015
    {
        List<IDrawable>? result = null;

        // Check layer styles
        var layerStyles = layer.Style?.GetStylesToApply(feature, viewport);
        if (layerStyles is not null)
        {
            foreach (var style in layerStyles)
            {
                if (!styleRenderers.TryGetValue(style.GetType(), out var renderer)
                    || renderer is not ITwoStepStyleRenderer twoStepRenderer)
                    continue;

#pragma warning disable IDISP001 // Dispose created - ownership transfers to caller/cache
                var drawable = twoStepRenderer.CreateDrawable(viewport, layer, feature, style, renderService);
#pragma warning restore IDISP001
                if (drawable is not null)
                {
                    result ??= [];
                    result.Add(drawable);
                }
            }
        }

        // Check feature styles
        if (feature.Styles is not null)
        {
            foreach (var style in feature.Styles)
            {
                if (!style.ShouldBeApplied(viewport.Resolution)) continue;

                if (!styleRenderers.TryGetValue(style.GetType(), out var renderer)
                    || renderer is not ITwoStepStyleRenderer twoStepRenderer)
                    continue;

#pragma warning disable IDISP001 // Dispose created - ownership transfers to caller/cache
                var drawable = twoStepRenderer.CreateDrawable(viewport, layer, feature, style, renderService);
#pragma warning restore IDISP001
                if (drawable is not null)
                {
                    result ??= [];
                    result.Add(drawable);
                }
            }
        }

        if (result is null) return null;
        return result.Count == 1 ? result[0] : new CompositeDrawable(result);
    }
}
