using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Rendering
{
    public class VisibleFeatureIterator
    {
        public static void IterateLayers(IViewport viewport, IEnumerable<ILayer> layers,
            Action<IViewport, IStyle, IFeature> callback)
        {
            foreach (var layer in layers)
            {
                IterateLayer(viewport, layer, callback);
            }
        }

        public static void IterateLayer(IViewport viewport, ILayer layer, 
            Action<IViewport, IStyle, IFeature> callback)
        {
            if (layer.Enabled == false) return;
            if (layer.MinVisible > viewport.Resolution) return;
            if (layer.MaxVisible < viewport.Resolution) return;

            if (layer is LabelLayer)
            {
                var labelLayer = layer as LabelLayer;
                //!!!labelLayer.UseLabelStacking ? LabelRenderer.RenderStackedLabelLayer(viewport, labelLayer) : LabelRenderer.RenderLabelLayer(viewport, labelLayer));
            }
            else
            {
                IterateVectorLayer(viewport, layer, callback);
            }
        }

        private static void IterateVectorLayer(IViewport viewport, ILayer layer,
            Action<IViewport, IStyle, IFeature> callback)
        {
            var features = layer.GetFeaturesInView(viewport.Extent, viewport.RenderResolution).ToList();

            var layerStyles = layer.Style is StyleCollection ? (layer.Style as StyleCollection).ToArray() : new [] {layer.Style};
            foreach (var layerStyle in layerStyles)
            {
                var style = layerStyle; // This is the default that could be overridden by an IThemeStyle

                foreach (var feature in features)
                {
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                    if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) || (style.MaxVisible < viewport.Resolution)) continue;

                    callback(viewport, style, feature);
                }
            }

            foreach (var feature in features)
            {
                var featureStyles = feature.Styles ?? Enumerable.Empty<IStyle>();
                foreach (var featureStyle in featureStyles)
                {
                    if (feature.Styles != null && featureStyle.Enabled)
                    {
                        callback(viewport, featureStyle, feature);
                    }
                }
            }
        }
    }
}
