using System;
using Mapsui.Geometries;
using Mapsui.Styles;

namespace Mapsui.Rendering.Xaml
{
    public static class MultiLineStringRenderer
    {
        public static System.Windows.Shapes.Shape Render(MultiLineString multiLineString, IStyle style, IReadOnlyViewport viewport)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            System.Windows.Shapes.Path path = LineStringRenderer.CreateLineStringPath(vectorStyle);
            path.Data = multiLineString.ToXaml();
            path.RenderTransform = new System.Windows.Media.MatrixTransform { Matrix = GeometryRenderer.CreateTransformMatrix(viewport) };
            GeometryRenderer.CounterScaleLineWidth(path, viewport.Resolution);
            return path;
        }
    }
}
