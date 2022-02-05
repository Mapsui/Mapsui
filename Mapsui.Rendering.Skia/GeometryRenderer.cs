using Mapsui.Logging;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public class GeometryRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, float layerOpacity,
            GeometryFeature geometryFeature, SymbolCache symbolCache)
        {
            Draw(canvas, viewport, style, layerOpacity, geometryFeature, geometryFeature.Geometry, symbolCache);
        }

        private static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, float layerOpacity,
            IFeature feature, Geometry? geometry, SymbolCache symbolCache)
        {
            if (geometry is Point point)
                PointRenderer.Draw(canvas, viewport, style, feature, point.X, point.Y, symbolCache,
                    layerOpacity * style.Opacity);
            else if (geometry is MultiPoint multiPoint)
                MultiPointRenderer.Draw(canvas, viewport, style, feature, multiPoint,
                    symbolCache, layerOpacity * style.Opacity);
            else if (geometry is LineString lineString)
                LineStringRenderer.Draw(canvas, viewport, style, feature, lineString,
                    layerOpacity * style.Opacity);
            else if (geometry is MultiLineString multiLineString)
                MultiLineStringRenderer.Draw(canvas, viewport, style, feature, multiLineString,
                    layerOpacity * style.Opacity);
            else if (geometry is Polygon polygon)
                PolygonRenderer.Draw(canvas, viewport, style, feature, polygon,
                    layerOpacity * style.Opacity, symbolCache);
            else if (geometry is MultiPolygon multiPolygon)
                MultiPolygonRenderer.Draw(canvas, viewport, style, feature, multiPolygon,
                    layerOpacity * style.Opacity, symbolCache);
            else if (geometry is GeometryCollection collection)
                for (var i = 0; i < collection.NumGeometries; i++)
                    Draw(canvas, viewport, style, layerOpacity, feature, collection.GetGeometryN(i), symbolCache);
            else
                Logger.Log(LogLevel.Warning,
                    $"Failed to find renderer for geometry feature of type {geometry?.GetType()}");
        }
    }
}
