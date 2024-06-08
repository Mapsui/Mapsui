using System;
using System.Collections.Generic;
using System.IO;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Rendering;

public interface IRenderer : IRenderInfo, IDisposable
{
    void Render(object target, Viewport viewport, IEnumerable<ILayer> layers, IEnumerable<IWidget> widgets, Color? background = null);
    MemoryStream RenderToBitmapStream(Viewport viewport, IEnumerable<ILayer> layers,
        Color? background = null, float pixelDensity = 1, IEnumerable<IWidget>? widgets = null, RenderFormat renderFormat = RenderFormat.Png);
    IRenderService RenderService { get; }
    IDictionary<Type, IWidgetRenderer> WidgetRenders { get; }
    IDictionary<Type, IStyleRenderer> StyleRenderers { get; }
    ImageSourceCache ImageSourceCache { get; }
}
