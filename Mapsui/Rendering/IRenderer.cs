using System;
using System.Collections.Generic;
using System.IO;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Rendering
{
    public interface IRenderer
    {
        void Render(object target, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, IEnumerable<IWidget> widgets, Color background = null);
        MemoryStream RenderToBitmapStream(IReadOnlyViewport viewport, IEnumerable<ILayer> layers, Color background = null);
        ISymbolCache SymbolCache { get; }
        IDictionary<Type, IWidgetRenderer> WidgetRenders { get; } 
    }
}