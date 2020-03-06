using Mapsui;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Rendering.Xaml;
using Mapsui.Rendering.Xaml.XamlStyles;
using Mapsui.Styles;
using System;
using System.Windows.Controls;
using XamlMedia = System.Windows.Media;
using XamlShapes = System.Windows.Shapes;
using XamlPoint = System.Windows.Point;
using XamlColors = System.Windows.Media.Colors;

public class XamlCustomStyleRenderer : IXamlStyleRenderer
{
    public static Random rnd = new Random();

    public bool Draw(Canvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache)
    {
        if (!(feature.Geometry is global::Mapsui.Geometries.Point worldPoint))
            return false;

        var screenPoint = viewport.WorldToScreen(worldPoint);

        var color = new XamlMedia.Color() { R = (byte)rnd.Next(0, 256), G = (byte)rnd.Next(0, 256), B = (byte)rnd.Next(0, 256), A = (byte)(256.0 * layer.Opacity * style.Opacity) };

        var path = new XamlShapes.Path
        {
            Fill = new XamlMedia.SolidColorBrush(color),
            Stroke = new XamlMedia.SolidColorBrush(XamlColors.Transparent),
            StrokeThickness = 0,
        };

        path.Data = new XamlMedia.EllipseGeometry
        {
            Center = new XamlPoint(0, 0),
            RadiusX = 20 * 0.5,
            RadiusY = 20 * 0.5
        };

        var matrix = XamlMedia.Matrix.Identity;
        MatrixHelper.InvertY(ref matrix);
        MatrixHelper.Translate(ref matrix, screenPoint.X, screenPoint.Y);

        canvas.Children.Add(path);

        return true;
    }
}
