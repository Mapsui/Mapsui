using Mapsui.Layers;
using Mapsui.Styles;
using System.Collections.Generic;

namespace Mapsui.Rendering;

/// <summary>
/// A style renderer that supports the two-step drawable architecture:
/// 1. CreateDrawables: Creates platform-specific drawable objects (can run on a background thread).
/// 2. DrawDrawable: Draws a pre-created drawable to the canvas (runs on the UI thread, should be fast).
/// </summary>
public interface IDrawableStyleRenderer : IStyleRenderer
{
    /// <summary>
    /// Creates drawable objects for the given feature and style. This method does the heavy work
    /// of creating platform-specific rendering objects (e.g. SKPath, SKPaint) and can be called
    /// on a background thread.
    /// </summary>
    /// <param name="viewport">The viewport at the time of creation (used for world coordinate extraction).</param>
    /// <param name="layer">The layer containing the feature.</param>
    /// <param name="feature">The feature to create drawables for.</param>
    /// <param name="style">The style to apply.</param>
    /// <param name="renderService">The render service for cache access.</param>
    /// <returns>A list of drawables representing the feature with the given style.</returns>
    IReadOnlyList<IDrawable> CreateDrawables(Viewport viewport, ILayer layer, IFeature feature, IStyle style,
        RenderService renderService);

    /// <summary>
    /// Draws a pre-created drawable to the canvas. This should be a fast operation that only
    /// applies viewport transforms and calls canvas draw methods.
    /// </summary>
    /// <param name="canvas">The platform-specific canvas (e.g. SKCanvas).</param>
    /// <param name="viewport">The current viewport (for world-to-screen transform).</param>
    /// <param name="drawable">The drawable to draw.</param>
    /// <param name="layer">The layer containing the feature (for opacity).</param>
    void DrawDrawable(object canvas, Viewport viewport, IDrawable drawable, ILayer layer);
}
