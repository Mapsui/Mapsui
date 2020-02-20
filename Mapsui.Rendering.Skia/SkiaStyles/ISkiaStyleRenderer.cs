using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Rendering.Skia
{
    public interface ISkiaStyleRenderer : IStyleRenderer
    {
        void Render(RenderStyleEventArgs args);
    }
}
