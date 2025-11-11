using Mapsui.Layers;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia;

public class CustomLayerRenderer
{
    public delegate void RenderHandler(SKCanvas canvas, Viewport viewport, ILayer layer, Mapsui.Rendering.RenderService renderService);

    public static void RenderLayer(SKCanvas canvas, Viewport viewport, ILayer layer, Mapsui.Rendering.RenderService renderService, RenderHandler renderHandler)
    {
        renderHandler(canvas, viewport, layer, renderService);
    }
}
