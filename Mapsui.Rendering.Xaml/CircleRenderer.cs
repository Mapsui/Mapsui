using Mapsui.Styles;
using System;
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
            var path = CreateCirclePath(viewport, circle, style as VectorStyle ?? new VectorStyle());
            var matrixTransform = new XamlMedia.MatrixTransform { Matrix = GeometryRenderer.CreateTransformMatrix1(viewport) };
            path.RenderTransform = matrixTransform;
            
            if (path.Fill != null)
                path.Fill.Transform = matrixTransform.Inverse as XamlMedia.MatrixTransform;

            path.UseLayoutRounding = true;
            path.IsHitTestVisible = false;

            return path;
        }

        private static XamlShapes.Shape CreateCirclePath(IViewport viewport, Circle circle, VectorStyle style)
        {
            // The SL StrokeThickness default is 1 which causes blurry bitmaps
            var path = new XamlShapes.Path
            {
                StrokeThickness = 2,
                Fill = ToXaml(style.Fill)
            };

            if (style.Outline != null)
            {
                path.Stroke = new XamlMedia.SolidColorBrush(style.Outline.Color.ToXaml());
                path.StrokeThickness = style.Outline.Width;
                //!!!path.StrokeDashArray = style.Outline.PenStyle.ToXaml();
                path.Tag = style.Outline.Width; // see #linewidthhack
            }

            // Get current position
            var position = Projection.SphericalMercator.ToLonLat(circle.X, circle.Y);

            // Calc ground resolution in meters per pixel of viewport for this latitude
            double groundResolution = Math.Cos(position.Y / 180.0 * Math.PI);

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