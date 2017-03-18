using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Styles;

namespace Mapsui.UI
{
    public static class InfoHelper
    {
        public static MouseInfoEventArgs GetInfoEventArgs(Map map, Point screenPosition, IEnumerable<ILayer> infoLayers,
            ISymbolCache symbolCache)
        {
            var worldPosition = map.Viewport.ScreenToWorld(new Point(screenPosition.X, screenPosition.Y));

            var feature = GetFeatureInfo(infoLayers, worldPosition, map.Viewport.Resolution, symbolCache);

            if (feature == null) return null;

            return new MouseInfoEventArgs { LayerName = "", Feature = feature };
        }

        private static IFeature GetFeatureInfo(IEnumerable<ILayer> layers, Point point, double resolution,
            ISymbolCache symbolCache)
        {
            foreach (var layer in layers)
            {
                if (layer.Enabled == false) continue;
                
                var allFeatures = layer.GetFeaturesInView(layer.Envelope, resolution);
                
                var features = allFeatures.Where(f => 
                    IsTouchingTakingIntoAccountSymbolStyles(point, f, layer.Style, resolution)).ToList();

                var feature = features.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Distance(point))
                    .FirstOrDefault();
                
                if (feature != null)
                {
                    return feature;
                }
            }
            return null;
        }

        private static bool IsTouchingTakingIntoAccountSymbolStyles(
            Point point, IFeature feature, IStyle layerStyle, double resolution)
        {
            if (feature.Geometry is Point)
            {
                var scale = 1.0;
                var style = layerStyle as SymbolStyle;
                if (style != null)
                {
                    scale = style.SymbolScale;
                }
                var marginX = SymbolStyle.DefaultWidth * 0.5 * resolution * scale;
                var marginY = SymbolStyle.DefaultHeight * 0.5 * resolution * scale;

                return feature.Geometry.Touches(point, marginX, marginY);
            }
            return feature.Geometry.Touches(point);
        }
    }
}