using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;

namespace Mapsui.Rendering
{
    public static class VisibleFeatureIterator
    {
        public static void IterateLayers(IReadOnlyViewport viewport, IEnumerable<ILayer> layers, long iteration,
            Action<IReadOnlyViewport, ILayer, IStyle, IFeature, float, long> callback)
        {
            foreach (var layer in layers)
            {
                if (layer.Enabled == false) continue;
                if (layer.MinVisible > viewport.Resolution) continue;
                if (layer.MaxVisible < viewport.Resolution) continue;

                IterateLayer(viewport, layer, iteration, callback);
            }
        }

        private static void IterateLayer(IReadOnlyViewport viewport, ILayer layer, long iteration, 
            Action<IReadOnlyViewport, ILayer, IStyle, IFeature, float, long> callback)
        {
            if (viewport.Extent == null) return;
            var features = layer.GetFeatures(viewport.Extent, viewport.Resolution).ToList();

            var layerStyles = layer.Style.GetStylesToApply(viewport.Resolution);
            foreach (var layerStyle in layerStyles)
            {
                var style = layerStyle; // This is the default that could be overridden by an IThemeStyle

                foreach (var feature in features)
                {
                    if (layerStyle is IThemeStyle themeStyle)
                    {
                        var styleForFeature = themeStyle.GetStyle(feature);
                        if (styleForFeature == null) continue;
                        style = styleForFeature;
                    }

                    if (!style.ShouldBeApplied(viewport.Resolution)) continue;

                    if (style is StyleCollection styleCollection) // The ThemeStyle can again return a StyleCollection
                    {
                        foreach (var s in styleCollection.Styles)
                        {
                            if (!s.ShouldBeApplied(viewport.Resolution)) continue;
                            callback(viewport, layer, s, feature, (float)layer.Opacity, iteration);
                        }
                    }
                    else
                    {
                        callback(viewport, layer, style, feature, (float)layer.Opacity, iteration);
                    }
                }
            }

            foreach (var feature in features)
            {
                var featureStyles = feature.Styles ?? Enumerable.Empty<IStyle>();
                foreach (var featureStyle in featureStyles)
                {
                    if (featureStyle is IThemeStyle themeStyle)
                    {
                        Logger.Log(LogLevel.Warning, $"The IFeature.Styles can not contain a {nameof(IThemeStyle)}");
                        continue;
                    }

                    if (featureStyle is StyleCollection styleCollection)
                    {
                        Logger.Log(LogLevel.Warning, $"The IFeature.Styles can not contain a {nameof(StyleCollection)}");
                        continue;
                    }

                    if (!featureStyle.ShouldBeApplied(viewport.Resolution)) continue;

                    callback(viewport, layer, featureStyle, feature, (float)layer.Opacity, iteration);
                }
            }
        }

    }
}
