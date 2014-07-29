using Mapsui.Layers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mapsui.Rendering.OpenTK
{
    class MapRenderer : IRenderer
    {
        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            throw new NotImplementedException();
        }

        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers)
        {
            throw new NotImplementedException();
        }
    }
}
