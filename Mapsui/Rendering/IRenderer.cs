
using System.Collections.Generic;
using SharpMap.Layers;

namespace SharpMap.Rendering
{
    public interface IRenderer
    {
        void Render(IViewport viewport, IEnumerable<ILayer> layers);
    }
}
