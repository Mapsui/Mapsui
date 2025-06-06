using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Rendering;

public static class VisibleFeatureIterator
{
    public static void IterateLayers(Viewport viewport, IEnumerable<ILayer> layers, long iteration,
        Action<Viewport, ILayer, IStyle, IFeature, float, long> callback, Action<ILayer>? customLayerRendererCallback = null)
    {
        foreach (var layer in layers)
        {
            if (layer.Enabled == false) continue;
            if (layer.MinVisible > viewport.Resolution) continue;
            if (layer.MaxVisible < viewport.Resolution) continue;

            if (layer.CustomLayerRendererName is not null && customLayerRendererCallback is not null)
                customLayerRendererCallback(layer);
            else
                IterateLayer(viewport, layer, iteration, callback);
        }
    }

    private static void IterateLayer(Viewport viewport, ILayer layer, long iteration,
        Action<Viewport, ILayer, IStyle, IFeature, float, long> callback)
    {
        var extent = viewport.ToExtent();
        if (extent is null) return;

        var features = layer.SortFeatures(layer.GetFeatures(extent, viewport.Resolution)).ToList();

        // Part 1. Styles on the layer
        var layerStyles = layer.Style.GetStylesToApply(viewport.Resolution);

        foreach (var layerStyle in layerStyles)
        {
            foreach (var feature in features)
            {
                if (layerStyle is IThemeStyle themeStyle)
                {
                    var stylesFromThemeStyle = themeStyle.GetStyle(feature, viewport).GetStylesToApply(viewport.Resolution);
                    foreach (var styleFromThemeStyle in stylesFromThemeStyle)
                    {
                        callback(viewport, layer, styleFromThemeStyle, feature, (float)layer.Opacity, iteration);
                    }
                }
                else
                {
                    callback(viewport, layer, layerStyle, feature, (float)layer.Opacity, iteration);
                }
            }
        }

        // Part 2. Styles on the feature
        foreach (var feature in features)
        {
            var featureStyles = feature.Styles ?? Enumerable.Empty<IStyle>();
            foreach (var featureStyle in featureStyles)
            {
                if (featureStyle is IThemeStyle themeStyle)
                {
                    Logger.Log(LogLevel.Warning, $"The IFeature.Styles can not contain a {nameof(IThemeStyle)}. Use {nameof(IThemeStyle)} on the layer");
                    continue;
                }

                if (featureStyle is StyleCollection styleCollection)
                {
                    Logger.Log(LogLevel.Warning, $"The IFeature.Styles can not contain a {nameof(StyleCollection)}. Use {nameof(StyleCollection)} on the layer");
                    continue;
                }

                if (!featureStyle.ShouldBeApplied(viewport.Resolution)) continue;

                callback(viewport, layer, featureStyle, feature, (float)layer.Opacity, iteration);
            }
        }
    }
}
