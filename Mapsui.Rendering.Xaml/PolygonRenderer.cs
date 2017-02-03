using System;
using Mapsui.Geometries;
using Mapsui.Styles;

namespace Mapsui.Rendering.Xaml
{
    public static class PolygonRenderer
    {
        public static System.Windows.Shapes.Shape RenderPolygon(Polygon polygon, IStyle style, IViewport viewport, BrushCache brushCache = null)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            System.Windows.Shapes.Path path = CreatePolygonPath(vectorStyle, viewport.Resolution, brushCache);
            path.Data = polygon.ToXaml();

            var matrixTransform = new System.Windows.Media.MatrixTransform { Matrix = GeometryRenderer.CreateTransformMatrix1(viewport) };
            path.RenderTransform = matrixTransform;

            if (path.Fill != null)
                path.Fill.Transform = matrixTransform.Inverse as System.Windows.Media.MatrixTransform;
            path.UseLayoutRounding = true;
            return path;
        }

        public static System.Windows.Shapes.Path CreatePolygonPath(VectorStyle style, double resolution, BrushCache brushCache = null)
        {
            var path = new System.Windows.Shapes.Path();

            if (style.Outline != null)
            {
                path.Stroke = new System.Windows.Media.SolidColorBrush(style.Outline.Color.ToXaml());
                path.StrokeThickness = style.Outline.Width * resolution;
                path.StrokeDashArray = style.Outline.PenStyle.ToXaml();
                path.Tag = style.Outline.Width; // see #linewidthhack
            }

            path.Fill = style.Fill.ToXaml(brushCache);
            path.IsHitTestVisible = false;
            return path;
        }

    }
}