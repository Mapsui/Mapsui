using System;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public class VectorStyleRenderer : ISkiaStyleRenderer
    {
        public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache, long iteration)
        {
            try
            {
                var vectorStyle = (VectorStyle)style;
                var opacity = (float)(layer.Opacity * style.Opacity);

                switch (feature)
                {
                    case (RectFeature rectFeature):
                        PolygonRenderer.Draw(canvas, viewport, style, rectFeature, rectFeature.Rect.ToPolygon(), opacity);
                        break;
                    case (PointFeature pointFeature):
                        // Use the SymbolStyleRenderer and specify Ellipse
                        var (destX, destY) = viewport.WorldToScreenXY(pointFeature.Point.X, pointFeature.Point.Y);
                        SymbolStyleRenderer.DrawSymbol(canvas, viewport, layer, pointFeature, new SymbolStyle { Outline = vectorStyle.Outline, Fill = vectorStyle.Fill, Line = vectorStyle.Line }, symbolCache, iteration);
                        break;
                    case (GeometryFeature geometryFeatureNts):
                        switch (geometryFeatureNts.Geometry)
                        {
                            case GeometryCollection collection:
                                for (var i = 0; i < collection.NumGeometries; i++)
                                    Draw(canvas, viewport, layer, new GeometryFeature(collection.GetGeometryN(i)), style, symbolCache, iteration);
                                break;
                            case Point point:
                                Draw(canvas, viewport, layer, new PointFeature(point.X, point.Y), style, symbolCache, iteration);
                                break;
                            case Polygon polygon:
                                PolygonRenderer.Draw(canvas, viewport, style, feature, polygon, opacity, symbolCache);
                                break;
                            case LineString lineString:
                                LineStringRenderer.Draw(canvas, viewport, style, feature, lineString, opacity);
                                break;
                            default:
                                throw new ArgumentException("Unknown geometry of Feature");
                        }
                        break;
                    case (GeometryCollection geometryFeatureCollection):
                        for (var i = 0; i < geometryFeatureCollection.NumGeometries; i++)
                            Draw(canvas, viewport, layer, new GeometryFeature(geometryFeatureCollection.GetGeometryN(i)), style, symbolCache, iteration);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }

            return true;
        }
    }
}