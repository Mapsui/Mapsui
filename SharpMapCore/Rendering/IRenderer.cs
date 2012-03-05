
using System.Collections.Generic;
using SharpMap.Layers;

namespace SharpMap.Rendering
{
    public interface IRenderer
    {
        void Render(IView view, IEnumerable<ILayer> layers);
    }
}
