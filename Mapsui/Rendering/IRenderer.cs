using System;
using System.Collections.Generic;
using System.IO;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Rendering;

public interface IRenderer : IRenderInfo
{
    void Render(object target, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, IEnumerable<IWidget> widgets, Color? background = null);
    MemoryStream RenderToBitmapStream(IReadOnlyViewport viewport, IEnumerable<ILayer> layers,
        Color? background = null, float pixelDensity = 1, IEnumerable<IWidget>? widgets = null, RenderFormat renderFormat = RenderFormat.Png);
    IRenderCache RenderCache { get; }
    IDictionary<Type, IWidgetRenderer> WidgetRenders { get; }
    IDictionary<Type, IStyleRenderer> StyleRenderers { get; }
}
