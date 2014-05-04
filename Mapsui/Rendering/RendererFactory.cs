using System;

namespace Mapsui.Rendering
{
    public static class RendererFactory
    {
        public static Func<IRenderer> Get { get; set; }
    }
}
