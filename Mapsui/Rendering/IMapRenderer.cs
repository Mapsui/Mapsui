using System;
using System.Collections.Generic;
using System.IO;
using Mapsui.Layers;
using Mapsui.Manipulations;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Rendering;

public interface IMapRenderer
{
    /// <summary>
    /// Updates pre-created drawable objects for a layer. Called when layer data changes.
    /// Implementations that support the two-step rendering architecture should create drawables here.
    /// </summary>
    /// <param name="viewport">The current viewport.</param>
    /// <param name="layer">The layer whose data has changed.</param>
    /// <param name="renderService">The render service (holds caches).</param>
    void UpdateDrawables(Viewport viewport, ILayer layer, RenderService renderService);

    /// <summary>
    /// Creates a drawable for a feature/style combination. This factory method is used
    /// by <see cref="RenderService.CreateDrawable"/> to allow the fetch pipeline to
    /// create drawables without needing access to renderer internals.
    /// </summary>
    /// <param name="viewport">The current viewport.</param>
    /// <param name="layer">The layer containing the feature.</param>
    /// <param name="feature">The feature to create a drawable for.</param>
    /// <param name="style">The style to apply.</param>
    /// <param name="renderService">The render service (holds caches).</param>
    /// <returns>A drawable, or null if this renderer doesn't support creating drawables for this style.</returns>
    IDrawable? CreateDrawableForFeature(Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService);

    /// <summary>
    /// Renders the map to the given target surface.
    /// </summary>
    /// <param name="target">The platform-specific render target (e.g. an <c>SKCanvas</c>).</param>
    /// <param name="viewport">The viewport describing the visible area and resolution.</param>
    /// <param name="layers">The layers to render, in draw order.</param>
    /// <param name="widgets">The widgets to draw on top of the map.</param>
    /// <param name="renderService">The render service, which holds shared caches and resources.</param>
    /// <param name="background">Optional background color to fill before drawing layers. Pass <see langword="null"/> to skip.</param>
    /// <param name="dirtyRegion">World-coordinate rectangle of the area that needs to be redrawn.
    /// Pass <see langword="null"/> to redraw the full viewport.
    /// When provided, only this region is repainted (canvas is clipped and feature queries are limited to this area).</param>
    void Render(object target, Viewport viewport, IEnumerable<ILayer> layers, IEnumerable<IWidget> widgets, RenderService renderService, Color? background = null, MRect? dirtyRegion = null);

    /// <summary>
    /// Renders the map to a PNG (or other format) bitmap and returns it as a stream.
    /// Useful for exporting the map or running rendering tests.
    /// </summary>
    /// <param name="viewport">The viewport describing the visible area and resolution.</param>
    /// <param name="layers">The layers to render.</param>
    /// <param name="renderService">The render service, which holds shared caches and resources.</param>
    /// <param name="background">Optional background color. Pass <see langword="null"/> to skip.</param>
    /// <param name="pixelDensity">Screen pixel density (e.g. 2 for a high-DPI display). Defaults to 1.</param>
    /// <param name="widgets">Optional widgets to include in the output image.</param>
    /// <param name="renderFormat">Output image format. Defaults to <see cref="RenderFormat.Png"/>.</param>
    /// <param name="quality">Compression quality for lossy formats (0–100). Ignored for PNG.</param>
    /// <returns>A <see cref="MemoryStream"/> containing the encoded image.</returns>
    MemoryStream RenderToBitmapStream(Viewport viewport, IEnumerable<ILayer> layers,
        RenderService renderService, Color? background = null, float pixelDensity = 1, IEnumerable<IWidget>? widgets = null, RenderFormat renderFormat = RenderFormat.Png, int quality = 100);

    /// <summary>
    /// Looks up the widget renderer registered for the given widget type.
    /// </summary>
    /// <param name="widgetType">The runtime type of the widget.</param>
    /// <param name="widgetRenderer">Set to the registered renderer, or <see langword="null"/> if none is found.</param>
    /// <returns><see langword="true"/> if a renderer was found; otherwise <see langword="false"/>.</returns>
    bool TryGetWidgetRenderer(Type widgetType, out IWidgetRenderer? widgetRenderer);

    /// <summary>
    /// Looks up the style renderer registered for the given style type.
    /// </summary>
    /// <param name="widgetType">The runtime type of the style.</param>
    /// <param name="widgetRenderer">Set to the registered renderer, or <see langword="null"/> if none is found.</param>
    /// <returns><see langword="true"/> if a renderer was found; otherwise <see langword="false"/>.</returns>
    bool TryGetStyleRenderer(Type widgetType, out IStyleRenderer? widgetRenderer);

    /// <summary>
    /// Returns information about the map feature(s) at the given screen position.
    /// Used to implement tap/click hit-testing on the map.
    /// </summary>
    /// <param name="screenPosition">The screen position to query (in pixels).</param>
    /// <param name="viewport">The current viewport.</param>
    /// <param name="layers">The layers to query.</param>
    /// <param name="renderService">The render service, which holds shared caches and resources.</param>
    /// <param name="margin">Extra hit area radius in pixels. Useful for small, hard-to-tap features.</param>
    /// <returns>A <see cref="MapInfo"/> describing what was found at that position.</returns>
    MapInfo GetMapInfo(ScreenPosition screenPosition, Viewport viewport, IEnumerable<ILayer> layers, RenderService renderService, int margin = 0);
}
