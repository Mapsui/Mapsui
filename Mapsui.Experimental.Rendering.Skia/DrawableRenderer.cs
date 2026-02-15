using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
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
    /// Updates drawables for a layer on a background thread. For each (feature, style) 
    /// combination that is not yet cached, calls <see cref="ITwoStepStyleRenderer.CreateDrawable"/> 
    /// and stores the result in the cache. Iterates in style-first order to match the
    /// rendering order used by VisibleFeatureIterator.
    /// Then calls <see cref="IDrawableCache.Cleanup"/> to evict stale entries.
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

            // Part 1: Layer styles (style-first order)
            var layerStyles = layer.Style.GetStylesToApply(viewport.Resolution);
            foreach (var layerStyle in layerStyles)
            {
                foreach (var feature in features)
                {
                    if (layerStyle is IThemeStyle themeStyle)
                    {
                        var stylesFromTheme = themeStyle.GetStyle(feature, viewport).GetStylesToApply(viewport.Resolution);
                        foreach (var style in stylesFromTheme)
                        {
                            CreateAndCacheDrawable(viewport, layer, feature, style, styleRenderers, renderService, cache, currentIteration);
                        }
                    }
                    else
                    {
                        CreateAndCacheDrawable(viewport, layer, feature, layerStyle, styleRenderers, renderService, cache, currentIteration);
                    }
                }
            }

            // Part 2: Feature styles (feature-first order, as in VisibleFeatureIterator)
            foreach (var feature in features)
            {
                if (feature.Styles is null) continue;
                foreach (var style in feature.Styles)
                {
                    if (style is IThemeStyle)
                    {
                        // ThemeStyles on features are not supported, skip (logged in VisibleFeatureIterator)
                        continue;
                    }
                    if (style is StyleCollection)
                    {
                        // StyleCollections on features are not supported, skip
                        continue;
                    }
                    if (!style.ShouldBeApplied(viewport.Resolution)) continue;

                    CreateAndCacheDrawable(viewport, layer, feature, style, styleRenderers, renderService, cache, currentIteration);
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
    /// Creates a drawable for a (feature, style) combination and caches it if not already cached.
    /// </summary>
    private static void CreateAndCacheDrawable(Viewport viewport, ILayer layer, IFeature feature, IStyle style,
        IReadOnlyDictionary<Type, IStyleRenderer> styleRenderers, RenderService renderService,
        IDrawableCache cache, long currentIteration)
    {
        if (!styleRenderers.TryGetValue(style.GetType(), out var renderer) ||
            renderer is not ITwoStepStyleRenderer twoStepRenderer)
            return;

        var key = new DrawableCacheKey(feature.GenerationId, style.GenerationId);

        // Check if already cached
        if (cache.Get(key, currentIteration) is not null)
            return;

#pragma warning disable IDISP001 // Dispose created - ownership transfers to cache via Set
        var drawable = twoStepRenderer.CreateDrawable(viewport, layer, feature, style, renderService);
#pragma warning restore IDISP001

        if (drawable is not null)
        {
            cache.Set(key, drawable, currentIteration);
        }
    }

    /// <summary>
    /// Tries to get a cached drawable for a (feature, style) combination. Called from the render thread.
    /// Stamps the cache entry with <paramref name="iteration"/> so that Cleanup
    /// knows the entry was recently used.
    /// </summary>
    public static IDrawable? TryGetDrawable(RenderService renderService, int layerId,
        long featureGenerationId, long styleGenerationId, long iteration)
    {
        var key = new DrawableCacheKey(featureGenerationId, styleGenerationId);
#pragma warning disable IDISP001, IDISP004 // Cache managed by RenderService
        return renderService.GetLayerDrawableCache(layerId).Get(key, iteration);
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
}
