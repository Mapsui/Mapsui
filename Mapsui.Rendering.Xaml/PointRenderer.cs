using Mapsui.Styles;
using Point = Mapsui.Geometries.Point;
using System.Windows;
using XamlMedia = System.Windows.Media;
using XamlShapes = System.Windows.Shapes;
using XamlPoint = System.Windows.Point;
using XamlColors = System.Windows.Media.Colors;

namespace Mapsui.Rendering.Xaml
{
    public static class PointRenderer
    {
        public static XamlShapes.Shape RenderPoint(Point point, IStyle style, IViewport viewport,
            SymbolCache symbolCache)
        {
            XamlShapes.Shape symbol;
            var matrix = XamlMedia.Matrix.Identity;

            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null)
            {
                if (symbolStyle.BitmapId < 0)
                    symbol = CreateSymbolFromVectorStyle(symbolStyle, symbolStyle.Opacity, symbolStyle.SymbolType);
                else
                    symbol = CreateSymbolFromBitmap(symbolStyle.BitmapId, symbolStyle.Opacity, symbolCache);
                matrix = CreatePointSymbolMatrix(viewport.Resolution, viewport.Rotation, symbolStyle);
            }
            else
            {
                symbol = CreateSymbolFromVectorStyle((style as VectorStyle) ?? new VectorStyle());
                MatrixHelper.ScaleAt(ref matrix, viewport.Resolution, viewport.Resolution);
            }

            MatrixHelper.Append(ref matrix, GeometryRenderer.CreateTransformMatrix(point, viewport));

            symbol.RenderTransform = new XamlMedia.MatrixTransform { Matrix = matrix };
            symbol.IsHitTestVisible = false;

            return symbol;
        }

        private static XamlShapes.Shape CreateSymbolFromVectorStyle(VectorStyle style, double opacity = 1,
            SymbolType symbolType = SymbolType.Ellipse)
        {
            // The SL StrokeThickness default is 1 which causes blurry bitmaps
            var path = new XamlShapes.Path
            {
                StrokeThickness = 0,
                Fill = ToXaml(style.Fill)
            };

            if (style.Outline != null)
            {
                path.Stroke = new XamlMedia.SolidColorBrush(style.Outline.Color.ToXaml());
                path.StrokeThickness = style.Outline.Width;
                path.StrokeDashArray = style.Outline.PenStyle.ToXaml();
            }

            if (symbolType == SymbolType.Ellipse)
                path.Data = CreateEllipse(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);
            else
                path.Data = CreateRectangle(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);

            path.Opacity = opacity;

            return path;
        }

        private static XamlMedia.Brush ToXaml(Brush brush)
        {
            return brush != null && (brush.Color != null || brush.BitmapId != -1) ?
                brush.ToXaml() : new XamlMedia.SolidColorBrush(XamlColors.Transparent);
        }

        private static XamlMedia.Matrix CreatePointSymbolMatrix(double resolution, double mapRotation, SymbolStyle symbolStyle)
        {
            var matrix = XamlMedia.Matrix.Identity;
            MatrixHelper.InvertY(ref matrix);
            var centerX = symbolStyle.SymbolOffset.X;
            var centerY = symbolStyle.SymbolOffset.Y;

            var scale = symbolStyle.SymbolScale;
            MatrixHelper.Translate(ref matrix, centerX, centerY);
            MatrixHelper.ScaleAt(ref matrix, scale, scale);

            //for point symbols we want the size to be independent from the resolution. We do this by counter scaling first.
            if (symbolStyle.UnitType != UnitType.WorldUnit)
                MatrixHelper.ScaleAt(ref matrix, resolution, resolution);

            MatrixHelper.RotateAt(ref matrix, mapRotation - symbolStyle.SymbolRotation);

            return matrix;
        }

        private static XamlShapes.Shape CreateSymbolFromBitmap(int bitmapId, double opacity, SymbolCache symbolCache)
        {
            var imageBrush = symbolCache.GetOrCreate(bitmapId).ToImageBrush();

            // note: It probably makes more sense to use PixelWidth here:
            var width = imageBrush.ImageSource.Width;
            var height = imageBrush.ImageSource.Height;

            var path = new XamlShapes.Path
            {
                Data = new XamlMedia.RectangleGeometry
                {
                    Rect = new Rect(-width * 0.5, -height * 0.5, width, height)
                },
                Fill = imageBrush,
                Opacity = opacity
            };

            return path;
        }

        private static XamlMedia.EllipseGeometry CreateEllipse(double width, double height)
        {
            return new XamlMedia.EllipseGeometry
            {
                Center = new XamlPoint(0, 0),
                RadiusX = width * 0.5,
                RadiusY = height * 0.5
            };
        }

        private static XamlMedia.RectangleGeometry CreateRectangle(double width, double height)
        {
            return new XamlMedia.RectangleGeometry
            {
                Rect = new Rect(width * -0.5, height * -0.5, width, height)
            };
        }

        public static void PositionPoint(UIElement renderedGeometry, Point point, IStyle style, IViewport viewport)
        {
            var matrix = XamlMedia.Matrix.Identity;
            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null) matrix = CreatePointSymbolMatrix(viewport.Resolution, viewport.Rotation, symbolStyle);
            else MatrixHelper.ScaleAt(ref matrix, viewport.Resolution, viewport.Resolution);
            MatrixHelper.Append(ref matrix, GeometryRenderer.CreateTransformMatrix(point, viewport));
            renderedGeometry.RenderTransform = new XamlMedia.MatrixTransform { Matrix = matrix };
        }
    }
}