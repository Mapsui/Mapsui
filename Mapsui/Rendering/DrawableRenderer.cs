using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Rendering;

/// <summary>
/// Static service that manages drawable creation and caching.
/// Drawable caches are stored in <see cref="RenderService"/> for centralized cache management.
/// 
/// Usage:
/// 1. Register style renderers (including <see cref="IDrawableStyleRenderer"/> implementations) in a single dictionary.
/// 2. Call <see cref="UpdateDrawables"/> when layer data changes (can run on a background thread).
/// 3. Call <see cref="TryGetDrawables"/> from the render thread to retrieve cached drawables.
/// </summary>
public static class DrawableRenderer
{
    /// <summary>
    /// Updates drawables for a layer by iterating its features and creating drawables
    /// for all styles that have a registered <see cref="IDrawableStyleRenderer"/> in the style renderers dictionary.
    /// This method can be called from a background thread.
    /// </summary>
    /// <param name="viewport">The current viewport.</param>
    /// <param name="layer">The layer to update drawables for.</param>
    /// <param name="styleRenderers">The unified registry of style renderers (may contain both legacy and drawable renderers).</param>
    /// <param name="renderService">The render service (holds the drawable caches).</param>
    public static void UpdateDrawables(Viewport viewport, ILayer layer,
        IReadOnlyDictionary<Type, IStyleRenderer> styleRenderers, RenderService renderService)
    {
        if (!styleRenderers.Values.Any(r => r is IDrawableStyleRenderer)) return;

        try
        {
            var cache = renderService.GetLayerDrawableCache(layer.Id);
            cache.Clear(); // Clear old drawables for this layer

            var extent = viewport.ToExtent();
            if (extent is null) return;

            var features = layer.GetFeatures(extent, viewport.Resolution);

            foreach (var feature in features)
            {
                UpdateDrawablesForFeature(viewport, layer, feature, styleRenderers, renderService, cache);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"Error updating drawables for layer '{layer.Name}': {ex.Message}", ex);
        }
    }

    private static void UpdateDrawablesForFeature(Viewport viewport, ILayer layer, IFeature feature,
        IReadOnlyDictionary<Type, IStyleRenderer> styleRenderers,
        RenderService renderService, DrawableCache cache)
    {
        // Check layer styles
        var layerStyles = layer.Style?.GetStylesToApply(viewport.Resolution);
        if (layerStyles is not null)
        {
            foreach (var style in layerStyles)
            {
                if (!styleRenderers.TryGetValue(style.GetType(), out var renderer)
                    || renderer is not IDrawableStyleRenderer drawableRenderer)
                    continue;

                var drawables = drawableRenderer.CreateDrawables(viewport, layer, feature, style, renderService);
                if (drawables.Count > 0)
                {
                    cache.Set(feature.Id, drawables);
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
                    || renderer is not IDrawableStyleRenderer drawableRenderer)
                    continue;

                var drawables = drawableRenderer.CreateDrawables(viewport, layer, feature, style, renderService);
                if (drawables.Count > 0)
                {
                    cache.Set(feature.Id, drawables);
                }
            }
        }
    }

    /// <summary>
    /// Tries to get cached drawables for a feature in a layer.
    /// Called from the render thread.
    /// </summary>
    /// <param name="renderService">The render service (holds the drawable caches).</param>
    /// <param name="layerId">The layer identifier.</param>
    /// <param name="featureId">The feature identifier.</param>
    /// <returns>The cached drawables, or null if not found.</returns>
    public static IReadOnlyList<IDrawable>? TryGetDrawables(RenderService renderService, int layerId, long featureId)
    {
        return renderService.GetLayerDrawableCache(layerId).Get(featureId);
    }
}
