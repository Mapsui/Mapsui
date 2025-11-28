using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Experimental.Rendering.Skia.SkiaStyles;
using Mapsui.Experimental.VectorTiles.Extensions;
using Mapsui.Experimental.VectorTiles.Tiling;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia;

public class VectorTileStyleRenderer(MapRenderer? mapRenderer = null) : ISkiaStyleRenderer
{
    private readonly MapRenderer _mapRenderer = mapRenderer ?? new MapRenderer();

    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, Mapsui.Rendering.RenderService renderService, long iteration)
    {
        if (feature is not VectorTileFeature vectorTileFeature)
        {
            Logger.Log(LogLevel.Warning, $"VectorTileStyleRenderer expected feature of type {nameof(VectorTileFeature)} but received {feature?.GetType().FullName ?? "null"} (Layer: {layer.Name}, Iteration: {iteration}).");
            return false;
        }
        if (style is not VectorTileStyle vectorTileStyle)
        {
            Logger.Log(LogLevel.Warning, $"VectorTileStyleRenderer expected style of type {nameof(VectorTileStyle)} but received {style?.GetType().FullName ?? "null"} (Layer: {layer.Name}, Iteration: {iteration}).");
            return false;
        }


        foreach (var vectorTileLayer in vectorTileFeature.VectorTile.Layers)
        {
            if (feature.Extent is null)
                continue;

            foreach (var ntsFeature in vectorTileLayer.Features)
            {
                var saveCount = canvas.Save();
                try
                {
                    var screenExtent = viewport.WorldToScreen(feature.Extent);

                    if (ntsFeature.Geometry is Polygon || ntsFeature.Geometry is MultiPolygon)
                    {
                        using var clipPath = screenExtent.ToSkiaPath();
                        canvas.ClipPath(clipPath);
                    }
                    var mapsuiFeature = ntsFeature.ToMapsui();

                    var featureStyles = vectorTileStyle.Style.GetStylesToApply(mapsuiFeature, viewport);
                    foreach (var featureStyle in featureStyles)
                    {
                        if (_mapRenderer.TryGetStyleRenderer(featureStyle.GetType(), out var styleRenderer))
                        {
                            var skiaStyleRenderer = styleRenderer as ISkiaStyleRenderer;
                            if (skiaStyleRenderer != null)
                            {
                                if (mapsuiFeature is Point)
                                    return false;
                                skiaStyleRenderer.Draw(canvas, viewport, layer, mapsuiFeature, featureStyle, renderService, iteration);
                            }
                            else
                            {
                                Logger.Log(LogLevel.Warning, $"Registered style renderer for style type {featureStyle.GetType().FullName} does not implement {nameof(ISkiaStyleRenderer)} (Layer: {layer.Name}, Iteration: {iteration}).");
                                continue;
                            }
                        }
                        else
                        {
                            Logger.Log(LogLevel.Warning, $"No style renderer registered for style type {featureStyle.GetType().FullName} (Layer: {layer.Name}, Iteration: {iteration}).");
                            continue;
                        }
                    }
                }
                finally
                {
                    canvas.RestoreToCount(saveCount);
                }
            }
        }
        return true;
    }
}
