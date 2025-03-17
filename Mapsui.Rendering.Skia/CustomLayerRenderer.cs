using Mapsui.Layers;
using Mapsui.Rendering.Skia.Cache;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

public class CustomLayerRenderer
{
    public delegate void RenderHandler(SKCanvas canvas, Viewport viewport, ILayer layer, RenderService renderService);

    public static void RenderLayer(SKCanvas canvas, Viewport viewport, ILayer layer, RenderService renderService, RenderHandler renderHandler)
    {
        renderHandler(canvas, viewport, layer, renderService);
    }
}
