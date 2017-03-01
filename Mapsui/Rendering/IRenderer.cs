using System.Collections.Generic;
using System.IO;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Rendering
{
    public interface IRenderer
    {
        void Render(object target, IViewport viewport, IEnumerable<ILayer> layers, Color background = null);
        MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers, Color background = null);
    }
}