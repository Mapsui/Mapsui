using Mapsui.Styles;
using Point = Mapsui.Geometries.Point;
using System;
using System.Windows;
using XamlMedia = System.Windows.Media;
using XamlShapes = System.Windows.Shapes;
using XamlPoint = System.Windows.Point;
using XamlColors = System.Windows.Media.Colors;
using Mapsui.Geometries;

namespace Mapsui.Rendering.Xaml
{
    public static class CircleRenderer
    {
        public static XamlShapes.Shape RenderCircle(Circle circle, IStyle style, IViewport viewport,
            SymbolCache symbolCache)
        {
            XamlShapes.Shape path;
            var matrix = XamlMedia.Matrix.Identity;

            path = CreateCirclePath(viewport, circle, (style as VectorStyle) ?? new VectorStyle());
            path.RenderTransform = new XamlMedia.MatrixTransform { Matrix = GeometryRenderer.CreateTransformMatrix1(viewport) };

            //MatrixHelper.ScaleAt(ref matrix, viewport.Resolution, viewport.Resolution);
            //MatrixHelper.Append(ref matrix, GeometryRenderer.CreateTransformMatrix(new Point(circle.X, circle.Y), viewport));
            //path.RenderTransform = new XamlMedia.MatrixTransform { Matrix = matrix };

            path.IsHitTestVisible = false;

            return path;
        }

        private static XamlShapes.Shape CreateCirclePath(IViewport viewport, Circle circle, VectorStyle style)
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

            // Get current position
            var position = Projection.SphericalMercator.ToLonLat(circle.X, circle.Y);

            // Calc ground resolution in meters per pixel of viewport for this latitude
            double groundResolution = viewport.Resolution * Math.Cos(position.Y / 180.0 * Math.PI);

            // Now we can calc the radius of circle
            var radius = circle.Radius / groundResolution;

            path.Data = new XamlMedia.EllipseGeometry
            {
                Center = new XamlPoint(circle.X, circle.Y),
                RadiusX = radius,
                RadiusY = radius
            };

            path.Opacity = 1;

            return path;
        }

        private static XamlMedia.Brush ToXaml(Brush brush)
        {
            return brush != null && (brush.Color != null || brush.BitmapId != -1) ?
                brush.ToXaml() : new XamlMedia.SolidColorBrush(XamlColors.Transparent);
        }
    }
}