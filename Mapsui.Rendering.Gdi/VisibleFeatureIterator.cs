// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
// Copyright 2010 - Paul den Dulk (Geodan) - Adapted SharpMap for Mapsui
//
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;

namespace Mapsui.Rendering.Gdi
{
    public class VisibleFeatureIterator
    {
        public static void IterateLayers(Graphics graphics, IViewport viewport, IEnumerable<ILayer> layers,
            Action<IViewport, IStyle, IFeature> callback)
        {
            foreach (var layer in layers)
            {
                IterateLayer(graphics, viewport, layer, callback);
            }
        }

        public static void IterateLayer(Graphics graphics, IViewport viewport, ILayer layer,
            Action<IViewport, IStyle, IFeature> callback)
        {
            if (layer.Enabled == false) return;
            if (layer.MinVisible > viewport.RenderResolution) return;
            if (layer.MaxVisible < viewport.RenderResolution) return;

            if (layer is LabelLayer)
            {
                LabellayerRenderer.Render(graphics, viewport, layer as LabelLayer);
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

            var layerStyles = layer.Style is StyleCollection ? (layer.Style as StyleCollection).ToArray() : new[] { layer.Style };
            foreach (var layerStyle in layerStyles)
            {
                var style = layerStyle; // This is the default that could be overridden by an IThemeStyle

                foreach (var feature in features)
                {
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                    if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.RenderResolution) || (style.MaxVisible < viewport.RenderResolution)) continue;

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
