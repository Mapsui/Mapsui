using Mapsui.Layers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaStyles
{
    public interface ISkiaStyleRenderer : IStyleRenderer
    {
        /// <summary>
        /// Drawing function for special styles
        /// </summary>
        /// <param name="canvas">SKCanvas for drawing</param>
        /// <param name="viewport">Active viewport for this drawing operation</param>
        /// <param name="layer">Layer that contains feature</param>
        /// <param name="feature">Feature to draw</param>
        /// <param name="style">Style to draw</param>
        /// <param name="symbolCache">SymbolCache for ready rendered bitmaps</param>
        /// <returns></returns>
        bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache, long iteration);
    }
}
