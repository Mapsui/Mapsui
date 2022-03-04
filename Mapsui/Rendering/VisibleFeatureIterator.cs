using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
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

            var layerStyles = ToArray(layer);
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

                    if (ShouldNotBeApplied(style, viewport)) continue;

                    if (style is StyleCollection styles) // The ThemeStyle can again return a StyleCollection
                    {
                        foreach (var s in styles)
                        {
                            if (ShouldNotBeApplied(s, viewport)) continue;
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
                var featureStyles = feature.Styles ?? Enumerable.Empty<IStyle>(); // null check
                foreach (var featureStyle in featureStyles)
                {
                    if (ShouldNotBeApplied(featureStyle, viewport)) continue;

                    callback(viewport, layer, featureStyle, feature, (float)layer.Opacity, iteration);

                }
            }
        }

        private static bool ShouldNotBeApplied(IStyle? style, IReadOnlyViewport viewport)
        {
            return style == null || !style.Enabled || style.MinVisible > viewport.Resolution || style.MaxVisible < viewport.Resolution;
        }

        private static IEnumerable<IStyle> ToArray(ILayer layer)
        {
            return (layer.Style as StyleCollection)?.ToArray() ??
                (layer.Style == null ? Array.Empty<IStyle>() : new[] { layer.Style });
        }
    }
}