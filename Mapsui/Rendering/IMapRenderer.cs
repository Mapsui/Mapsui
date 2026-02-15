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
    void Render(object target, Viewport viewport, IEnumerable<ILayer> layers, IEnumerable<IWidget> widgets, RenderService renderService, Color? background = null);
    MemoryStream RenderToBitmapStream(Viewport viewport, IEnumerable<ILayer> layers,
        RenderService renderService, Color? background = null, float pixelDensity = 1, IEnumerable<IWidget>? widgets = null, RenderFormat renderFormat = RenderFormat.Png, int quality = 100);
    bool TryGetWidgetRenderer(Type widgetType, out IWidgetRenderer? widgetRenderer);
    bool TryGetStyleRenderer(Type widgetType, out IStyleRenderer? widgetRenderer);
    MapInfo GetMapInfo(ScreenPosition screenPosition, Viewport viewport, IEnumerable<ILayer> layers, RenderService renderService, int margin = 0);
}
