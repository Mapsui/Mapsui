
using System.Collections.Generic;
using Mapsui.Layers;

namespace Mapsui.Rendering
{
    public interface IRenderer
    {
        void Render(IViewport viewport, IEnumerable<ILayer> layers);
    }
}
