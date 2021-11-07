using System;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class RectRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle? style, RectFeature rectFeature,
            float opacity)
        {
            try
            {
                PolygonRenderer.Draw(canvas, viewport, style, rectFeature, rectFeature.Rect.ToPolygon(), opacity);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        }
    }
}