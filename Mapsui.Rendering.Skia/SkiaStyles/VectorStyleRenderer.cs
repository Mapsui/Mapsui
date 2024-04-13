using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Nts;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;
using System;

namespace Mapsui.Rendering.Skia;

public class VectorStyleRenderer : ISkiaStyleRenderer, IFeatureSize
{
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, IRenderService renderService, long iteration)
    {
        try
        {
            var vectorStyle = (VectorStyle)style;
            var opacity = (float)(layer.Opacity * style.Opacity);

            switch (feature)
            {
                case PointFeature pointFeature:
                    SymbolStyleRenderer.DrawXY(canvas, viewport, layer, pointFeature.Point.X, pointFeature.Point.Y, CreateSymbolStyle(vectorStyle), renderService);
                    break;
                case GeometryFeature geometryFeature:
                    switch (geometryFeature.Geometry)
                    {
                        case GeometryCollection collection:
                            GeometryCollectionRenderer.Draw(canvas, viewport, vectorStyle, feature, collection, opacity, renderService.VectorCache);
                            break;
                        case Point point:
                            SymbolStyleRenderer.DrawXY(canvas, viewport, layer, point.X, point.Y, CreateSymbolStyle(vectorStyle), renderService);
                            break;
                        case Polygon polygon:
                            PolygonRenderer.Draw(canvas, viewport, vectorStyle, feature, polygon, opacity, renderService.VectorCache);
                            break;
                        case LineString lineString:
                            LineStringRenderer.Draw(canvas, viewport, vectorStyle, feature, lineString, opacity, renderService);
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

    double IFeatureSize.FeatureSize(IStyle style, IRenderService renderService, IFeature? feature)
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
