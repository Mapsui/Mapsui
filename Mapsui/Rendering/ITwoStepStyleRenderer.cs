using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Rendering;

/// <summary>
/// Optional interface that style renderers can implement to support two-step rendering
/// with background preparation. The caching is managed externally by the drawable renderer
/// orchestrator (background thread) and the map renderer (render thread).
/// Renderers that implement this interface do NOT interact with the cache directly.
///
/// When a renderer implements this interface:
/// 1. <see cref="CreateCache"/> is called once per layer to create the appropriate cache type.
/// 2. <see cref="CreateDrawable"/> is called on a background thread — the caller stores the
///    returned drawable in the cache.
/// 3. On the render thread, the caller fetches the cached drawable and passes it to
///    <see cref="DrawDrawable"/>.
///
/// Renderers that do NOT implement this interface work as before — everything runs
/// on the render thread via <c>ISkiaStyleRenderer.Draw</c>.
/// </summary>
public interface ITwoStepStyleRenderer : IStyleRenderer
{
    /// <summary>
    /// Creates an <see cref="IDrawableCache"/> appropriate for this renderer's caching strategy.
    /// Called once per layer. For example, tile renderers return a <see cref="TileDrawableCache"/>
    /// (LRU), while regular feature renderers return a <see cref="DrawableCache"/> (strict reconciliation).
    /// </summary>
    IDrawableCache CreateCache();

    /// <summary>
    /// Creates a drawable object for the given feature on a background thread.
    /// The caller is responsible for storing the result in the layer's cache.
    /// Returns null if the feature cannot be rendered.
    /// </summary>
    IDrawable? CreateDrawable(Viewport viewport, ILayer layer, IFeature feature,
        IStyle style, RenderService renderService);

    /// <summary>
    /// Draws a single pre-created drawable to the canvas on the render thread.
    /// This is the fast step — it should only blit/transform, not allocate expensive objects.
    /// </summary>
    void DrawDrawable(object canvas, Viewport viewport, IDrawable drawable, ILayer layer);
}
