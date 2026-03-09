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

    void Render(object target, Viewport viewport, IEnumerable<ILayer> layers, IEnumerable<IWidget> widgets, RenderService renderService, Color? background = null, MRect? dirtyRegion = null);
    MemoryStream RenderToBitmapStream(Viewport viewport, IEnumerable<ILayer> layers,
        RenderService renderService, Color? background = null, float pixelDensity = 1, IEnumerable<IWidget>? widgets = null, RenderFormat renderFormat = RenderFormat.Png, int quality = 100);
    bool TryGetWidgetRenderer(Type widgetType, out IWidgetRenderer? widgetRenderer);
    bool TryGetStyleRenderer(Type widgetType, out IStyleRenderer? widgetRenderer);
    MapInfo GetMapInfo(ScreenPosition screenPosition, Viewport viewport, IEnumerable<ILayer> layers, RenderService renderService, int margin = 0);
}
