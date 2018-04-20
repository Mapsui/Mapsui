using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Rendering
{
    public static class VisibleFeatureIterator
    {
        public static void IterateLayers(IViewport viewport, IEnumerable<ILayer> layers,
            Action<IViewport, IStyle, IFeature, float> callback)
        {
            foreach (var layer in layers)
            {
                if (layer.Enabled == false) continue;
                if (layer.MinVisible > viewport.Resolution) continue;
                if (layer.MaxVisible < viewport.Resolution) continue;

                IterateLayer(viewport, layer, callback);
            }
        }

        private static void IterateLayer(IViewport viewport, ILayer layer,
            Action<IViewport, IStyle, IFeature, float> callback)
        {
            var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToList();

            var layerStyles = ToArray(layer);
            foreach (var layerStyle in layerStyles)
            {
                var style = layerStyle; // This is the default that could be overridden by an IThemeStyle

                foreach (var feature in features)
                {
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                    if (style == null || style.Enabled == false || style.MinVisible > viewport.Resolution || style.MaxVisible < viewport.Resolution) continue;

                    if (style is StyleCollection styles) // The ThemeStyle can again return a StyleCollection
                    {
                        foreach (var s in styles)
                        {
                            callback(viewport, s, feature, (float)layer.Opacity);
                        }
                    }
                    else
                    {
                        callback(viewport, style, feature, (float)layer.Opacity);
                    }
                }
            }

            foreach (var feature in features)
            {
                var featureStyles = feature.Styles ?? Enumerable.Empty<IStyle>(); // null check
                foreach (var featureStyle in featureStyles)
                {
                    if (feature.Styles != null && featureStyle.Enabled)
                    {
                        callback(viewport, featureStyle, feature, (float)layer.Opacity);
                    }
                }
            }
        }

        private static IStyle[] ToArray(IStyle style)
        {
            return (style as StyleCollection)?.ToArray() ?? new[] { style };
        }

        private static IStyle[] ToArray(ILayer layer)
        {
            return (layer.Style as StyleCollection)?.ToArray() ?? new [] {layer.Style};
        }
    }
}