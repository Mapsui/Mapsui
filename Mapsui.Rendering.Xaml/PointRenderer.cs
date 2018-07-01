using Mapsui.Styles;
using Point = Mapsui.Geometries.Point;
using System;
using System.Windows;
using XamlMedia = System.Windows.Media;
using XamlShapes = System.Windows.Shapes;
using XamlPoint = System.Windows.Point;
using XamlColors = System.Windows.Media.Colors;

namespace Mapsui.Rendering.Xaml
{
    public static class PointRenderer
    {
        public static XamlShapes.Shape RenderPoint(Point point, IStyle style, IReadOnlyViewport viewport,
            SymbolCache symbolCache)
        {
            XamlShapes.Shape symbol;
            var matrix = XamlMedia.Matrix.Identity;

            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null)
            {
                if (symbolStyle.BitmapId < 0)
                    symbol = CreateSymbolFromVectorStyle(symbolStyle, symbolStyle.Opacity, symbolStyle.SymbolType, symbolCache, (float)viewport.Rotation);
                else
                {
                    symbol = CreateSymbolFromBitmap(symbolStyle.BitmapId, symbolStyle.Opacity, symbolCache);
                }
                matrix = CreatePointSymbolMatrix(viewport.Resolution, viewport.Rotation, symbolStyle, symbol.Width, symbol.Height);
            }
            else
            {
                symbol = CreateSymbolFromVectorStyle((style as VectorStyle) ?? new VectorStyle(), symbolCache: symbolCache, rotate: (float)viewport.Rotation);
                MatrixHelper.ScaleAt(ref matrix, viewport.Resolution, viewport.Resolution);
            }

            MatrixHelper.Append(ref matrix, GeometryRenderer.CreateTransformMatrix(viewport, point));

            symbol.RenderTransform = new XamlMedia.MatrixTransform { Matrix = matrix };
            symbol.IsHitTestVisible = false;

            return symbol;
        }

        private static XamlShapes.Shape CreateSymbolFromVectorStyle(VectorStyle style, double opacity = 1,
            SymbolType symbolType = SymbolType.Ellipse, SymbolCache symbolCache = null, float rotate = 0f)
        {
            // The SL StrokeThickness default is 1 which causes blurry bitmaps
            var path = new XamlShapes.Path
            {
                StrokeThickness = 0,
                Fill = ToXaml(style.Fill, symbolCache, rotate)
            };

            if (style.Outline != null)
            {
                path.Stroke = new XamlMedia.SolidColorBrush(style.Outline.Color.ToXaml());
                path.StrokeThickness = style.Outline.Width;
                path.StrokeDashArray = style.Outline.PenStyle.ToXaml(style.Outline.DashArray);
            }

            switch (symbolType)
            {
                case SymbolType.Ellipse:
                    path.Data = CreateEllipse(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);
                    break;
                case SymbolType.Rectangle:
                    path.Data = CreateRectangle(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);
                    break;
                case SymbolType.Triangle:
                    path.Data = CreateTriangle(SymbolStyle.DefaultWidth);
                    break;
                default: // Invalid value
                    throw new ArgumentOutOfRangeException();
            }                

            path.Opacity = opacity;

            return path;
        }

        private static XamlMedia.Brush ToXaml(Brush brush, SymbolCache symbolCache, float rotate = 0f)
        {
            return brush != null && (brush.Color != null || brush.BitmapId != -1) ?
                brush.ToXaml(symbolCache, rotate) : new XamlMedia.SolidColorBrush(XamlColors.Transparent);
        }

        private static XamlMedia.Matrix CreatePointSymbolMatrix(double resolution, double mapRotation, SymbolStyle symbolStyle, double width, double height)
        {
            var matrix = XamlMedia.Matrix.Identity;
            MatrixHelper.InvertY(ref matrix);

            var centerX = symbolStyle.SymbolOffset.IsRelative ? width * symbolStyle.SymbolOffset.X : symbolStyle.SymbolOffset.X;
            var centerY = symbolStyle.SymbolOffset.IsRelative ? height * symbolStyle.SymbolOffset.Y : symbolStyle.SymbolOffset.Y;

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

            var size = symbolCache.GetSize(bitmapId);

            // note: It probably makes more sense to use PixelWidth here:
            var width = size.Width;
            var height = size.Height;

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

        /// <summary>
        /// Equilateral triangle of side 'sideLength', centered on the same point as if a circle of diameter 'sideLength' was there
        /// </summary>
        private static XamlMedia.PathGeometry CreateTriangle(double sideLength)
        {
            var altitude = Math.Sqrt(3) / 2.0 * sideLength;
            var inradius = altitude / 3.0;
            var circumradius = 2.0 * inradius;

            var top = new XamlPoint(0, -circumradius);
            var left = new XamlPoint(sideLength * -0.5, inradius);
            var right = new XamlPoint(sideLength * 0.5, inradius);

            var segments = new XamlMedia.PathSegmentCollection();
            segments.Add(new XamlMedia.LineSegment(left, true));
            segments.Add(new XamlMedia.LineSegment(right, true));
            var figure = new XamlMedia.PathFigure(top, segments, true);
            var figures = new XamlMedia.PathFigureCollection();
            figures.Add(figure);

            return new XamlMedia.PathGeometry
            {
                Figures = figures
            };
        }

        public static void PositionPoint(UIElement renderedGeometry, Point point, IStyle style, IReadOnlyViewport viewport)
        {
            var matrix = XamlMedia.Matrix.Identity;
            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null) matrix = CreatePointSymbolMatrix(viewport.Resolution, viewport.Rotation, symbolStyle, renderedGeometry.RenderSize.Width, renderedGeometry.RenderSize.Height);
            else MatrixHelper.ScaleAt(ref matrix, viewport.Resolution, viewport.Resolution);
            MatrixHelper.Append(ref matrix, GeometryRenderer.CreateTransformMatrix(viewport, point));
            renderedGeometry.RenderTransform = new XamlMedia.MatrixTransform { Matrix = matrix };
        }
    }
}