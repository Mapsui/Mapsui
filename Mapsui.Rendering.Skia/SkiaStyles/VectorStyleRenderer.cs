using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;
using System;

namespace Mapsui.Rendering.Skia;

public class VectorStyleRenderer : ISkiaStyleRenderer
{
    public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, IRenderCache renderCache, long iteration)
    {
        try
        {
            var vectorStyle = (VectorStyle)style;
            var opacity = (float)(layer.Opacity * style.Opacity);

            switch (feature)
            {
                case RectFeature rectFeature:
                    if (rectFeature.Rect != null)
                        PolygonRenderer.Draw(canvas, viewport, vectorStyle, rectFeature, rectFeature.Rect.ToPolygon(), opacity, renderCache, renderCache);
                    break;
                case PointFeature pointFeature:
                    SymbolStyleRenderer.DrawSymbol(canvas, viewport, layer, pointFeature.Point.X, pointFeature.Point.Y, new SymbolStyle { Outline = vectorStyle.Outline, Fill = vectorStyle.Fill, Line = vectorStyle.Line });
                    break;
                case GeometryFeature geometryFeature:
                    switch (geometryFeature.Geometry)
                    {
                        case GeometryCollection collection:
                            for (var i = 0; i < collection.NumGeometries; i++)
                                Draw(canvas, viewport, layer, new GeometryFeature(collection.GetGeometryN(i)), style, renderCache, iteration);
                            break;
                        case Point point:
                            Draw(canvas, viewport, layer, new PointFeature(point.X, point.Y), style, renderCache, iteration);
                            break;
                        case Polygon polygon:
                            PolygonRenderer.Draw(canvas, viewport, vectorStyle, feature, polygon, opacity, renderCache, renderCache);
                            break;
                        case LineString lineString:
                            LineStringRenderer.Draw(canvas, viewport, vectorStyle, lineString, opacity, renderCache);
                            break;
                        case null:
                            throw new ArgumentException($"Geometry is null, Layer: {layer.Name}");
                        default:
                            throw new ArgumentException($"Unknown geometry type: {geometryFeature.Geometry?.GetType()}, Layer: {layer.Name}");
                    }
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
