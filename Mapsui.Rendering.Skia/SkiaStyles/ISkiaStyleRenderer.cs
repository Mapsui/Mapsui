﻿using Mapsui.Layers;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaStyles;

public interface ISkiaStyleRenderer : IStyleRenderer
{
    /// <summary>
    /// Drawing function for special styles
    /// </summary>
    /// <param name="canvas">SKCanvas for drawing.</param>
    /// <param name="viewport">Active viewport for this drawing operation.</param>
    /// <param name="layer">Layer that contains feature.</param>
    /// <param name="feature">Feature to draw.</param>
    /// <param name="style">Style to draw.</param>
    /// <param name="renderService">RenderService to provide caches and such.</param>
    /// <returns></returns>
    bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService, long iteration);
}
