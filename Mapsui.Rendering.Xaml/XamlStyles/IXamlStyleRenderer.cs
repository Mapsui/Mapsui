using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using System.Windows.Controls;

namespace Mapsui.Rendering.Xaml.XamlStyles
{
    public interface IXamlStyleRenderer : IStyleRenderer
    {
        /// <summary>
        /// Drawing function for special styles
        /// </summary>
        /// <param name="canvas">Canvas for drawing</param>
        /// <param name="viewport">Active viewport for this drawing operation</param>
        /// <param name="layer">Layer that contains feature</param>
        /// <param name="feature">Feature to draw</param>
        /// <param name="style">Style to draw</param>
        /// <param name="symbolCache">SymbolCache for ready rendered bitmaps</param>
        /// <returns></returns>
        bool Draw(Canvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache);
    }
}
