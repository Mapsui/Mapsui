using System;
using Mapsui.Geometries;
using Mapsui.Styles;

namespace Mapsui.Rendering.Xaml
{
    public static class MultiPolygonRenderer
    {
        public static System.Windows.Shapes.Path RenderMultiPolygon(MultiPolygon geometry, IStyle style,
            IReadOnlyViewport viewport, SymbolCache symbolCache)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = (VectorStyle) style;
            var path = PolygonRenderer.CreatePolygonPath(vectorStyle, viewport.Resolution, symbolCache);
            path.Data = geometry.ToXaml();
            var matrixTransform = new System.Windows.Media.MatrixTransform { Matrix = GeometryRenderer.CreateTransformMatrix(viewport) };
            path.RenderTransform = matrixTransform;

            if (path.Fill != null)
                path.Fill.Transform = matrixTransform.Inverse as System.Windows.Media.MatrixTransform;

            return path;
        }
    }
}
