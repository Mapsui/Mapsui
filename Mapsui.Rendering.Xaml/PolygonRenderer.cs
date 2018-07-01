using System;
using Mapsui.Geometries;
using Mapsui.Styles;

namespace Mapsui.Rendering.Xaml
{
    public static class PolygonRenderer
    {
        public static System.Windows.Shapes.Shape RenderPolygon(Polygon polygon, IStyle style, IReadOnlyViewport viewport, SymbolCache symbolCache)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            System.Windows.Shapes.Path path = CreatePolygonPath(vectorStyle, viewport.Resolution, symbolCache, (float)viewport.Rotation);
            path.Data = polygon.ToXaml();

            var matrixTransform = new System.Windows.Media.MatrixTransform { Matrix = GeometryRenderer.CreateTransformMatrix(viewport) };
            path.RenderTransform = matrixTransform;

            if (path.Fill != null)
                path.Fill.Transform = matrixTransform.Inverse as System.Windows.Media.MatrixTransform;
            path.UseLayoutRounding = true;
            return path;
        }

        public static System.Windows.Shapes.Path CreatePolygonPath(VectorStyle style, double resolution, SymbolCache symbolCache, float rotate = 0f)
        {
            var path = new System.Windows.Shapes.Path { Opacity = style.Opacity };

            if (style.Outline != null)
            {
                path.Stroke = new System.Windows.Media.SolidColorBrush(style.Outline.Color.ToXaml());
                path.StrokeThickness = style.Outline.Width * resolution;
                path.StrokeDashArray = style.Outline.PenStyle.ToXaml(style.Outline.DashArray);
                path.StrokeLineJoin = style.Outline.StrokeJoin.ToXaml();
                path.StrokeMiterLimit = style.Outline.StrokeMiterLimit;
                path.Tag = style.Outline.Width; // see #linewidthhack
            }

            path.Fill = style.Fill.ToXaml(symbolCache, rotate);
            path.IsHitTestVisible = false;
            return path;
        }

    }
}