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

public class VectorStyleRenderer : ISkiaStyleRenderer, IFeatureSize
{
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, IRenderCache renderCache, long iteration)
    {
        try
        {
            var vectorStyle = (VectorStyle)style;
            var opacity = (float)(layer.Opacity * style.Opacity);

            switch (feature)
            {
                case RectFeature rectFeature:
                    if (rectFeature.Rect != null)
                        PolygonRenderer.Draw(canvas, viewport, layer, vectorStyle, rectFeature, rectFeature.Rect.ToPolygon(), opacity, renderCache, renderCache);
                    break;
                case PointFeature pointFeature:
                    SymbolStyleRenderer.DrawSymbol(canvas, viewport, layer, pointFeature.Point.X, pointFeature.Point.Y, CreateSymbolStyle(vectorStyle), renderCache);
                    break;
                case GeometryFeature geometryFeature:
                    switch (geometryFeature.Geometry)
                    {
                        case GeometryCollection collection:
                            GeometryCollectionRenderer.Draw(canvas, viewport, layer, vectorStyle, feature, collection, opacity, renderCache);
                            break;
                        case Point point:
                            SymbolStyleRenderer.DrawSymbol(canvas, viewport, layer, point.X, point.Y, CreateSymbolStyle(vectorStyle), renderCache);
                            break;
                        case Polygon polygon:
                            PolygonRenderer.Draw(canvas, viewport, layer, vectorStyle, feature, polygon, opacity, renderCache, renderCache);
                            break;
                        case LineString lineString:
                            LineStringRenderer.Draw(canvas, viewport, layer, vectorStyle, feature, lineString, opacity, renderCache);
                            break;
                        case null:
                            throw new ArgumentException($"Geometry is null, Layer: {layer.Name}");
                        default:
                            throw new ArgumentException($"Unknown geometry type: {geometryFeature.Geometry?.GetType()}, Layer: {layer.Name}");
                    }
                    break;
                default: 
                    Logger.Log(LogLevel.Warning, $"{nameof(VectorStyleRenderer)} can not render feature of type '{feature.GetType()}', Layer: {layer.Name}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }

        return true;
    }

    private static SymbolStyle CreateSymbolStyle(VectorStyle vectorStyle)
    {
        return new SymbolStyle { Outline = vectorStyle.Outline, Fill = vectorStyle.Fill, Line = vectorStyle.Line };
    }

    bool IFeatureSize.NeedsFeature => false;

    double IFeatureSize.FeatureSize(IStyle style, IRenderCache renderCache, IFeature? feature)
    {
        if (style is VectorStyle vectorStyle)
        {
            return FeatureSize(vectorStyle);
        }

        return 0;
    }

    public static double FeatureSize(VectorStyle vectorStyle)
    {
        var size = Math.Max(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);
        double lineSize = 1;
        if (vectorStyle.Line != null)
        {
            lineSize = Math.Max(lineSize, vectorStyle.Line.Width);
        }

        if (vectorStyle.Outline != null)
        {
            lineSize = Math.Max(lineSize, vectorStyle.Outline.Width);
        }

        // add line size.
        size += lineSize;

        return size;
    }
}
