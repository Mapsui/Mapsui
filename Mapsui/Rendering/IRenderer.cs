using System.Collections.Generic;
using System.IO;
using Mapsui.Layers;

namespace Mapsui.Rendering
{
    public interface IRenderer
    {
        void Render(IViewport viewport, IEnumerable<ILayer> layers);
        MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers);
    }
}
