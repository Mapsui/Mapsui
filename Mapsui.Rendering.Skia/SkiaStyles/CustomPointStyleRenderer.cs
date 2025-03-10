using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Styles;
using SkiaSharp;
using System;

namespace Mapsui.Rendering.Skia.SkiaStyles;

public class CustomPointStyleRenderer : ISkiaStyleRenderer
{
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService, long iteration)
    {
        if (style is CustomPointStyle customPointStyle)
        {
            feature.CoordinateVisitor((x, y, setter) =>
            {
                var opacity = (float)(layer.Opacity * customPointStyle.Opacity);
                if (MapRenderer.TryGetPointStyleRenderer(customPointStyle.RendererName, out var pointStyleRenderer))
                    PointStyleRenderer.DrawPointStyle(canvas, viewport, x, y, customPointStyle, renderService, opacity, pointStyleRenderer);
                else
                    Logger.Log(LogLevel.Error, $"Could not find the point style renderer with name {customPointStyle.RendererName}");
            });
            return true;
        }
        else
            throw new ArgumentException($"Parameter style is not a {nameof(CustomPointStyle)}");
    }
}
