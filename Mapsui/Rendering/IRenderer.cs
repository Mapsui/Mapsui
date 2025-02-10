using System;
using System.Collections.Generic;
using System.IO;
using Mapsui.Layers;
using Mapsui.Manipulations;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Rendering;

public interface IRenderer : IDisposable
{
    void Render(object target, Viewport viewport, IEnumerable<ILayer> layers, IEnumerable<IWidget> widgets, Color? background = null);
    MemoryStream RenderToBitmapStream(Viewport viewport, IEnumerable<ILayer> layers,
        Color? background = null, float pixelDensity = 1, IEnumerable<IWidget>? widgets = null, RenderFormat renderFormat = RenderFormat.Png, int quality = 100);
    IRenderService RenderService { get; }
    bool TryGetWidgetRenderer(Type widgetType, out IWidgetRenderer? widgetRenderer);
    bool TryGetStyleRenderer(Type widgetType, out IStyleRenderer? widgetRenderer);
    ImageSourceCache ImageSourceCache { get; }
    MapInfo GetMapInfo(ScreenPosition screenPosition, Viewport viewport, IEnumerable<ILayer> layers, int margin = 0);

}
