using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Logging;
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
                    IsTouchingTakingIntoAccountSymbolStyles(point, f, layer.Style, resolution, symbolCache)).ToList();

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
            Point point, IFeature feature, IStyle layerStyle, double resolution, ISymbolCache symbolCache)
        {
            if (feature.Geometry is Point)
            {
                var styles = new List<IStyle>();
                if (layerStyle != null) styles.Add(layerStyle);
                styles.AddRange(feature.Styles);

                foreach (var style in styles)
                {
                    var symbolStyle = style as SymbolStyle;

                    if (symbolStyle == null)
                    {
                        Logger.Log(LogLevel.Warning, $"Feature info no supported for {style.GetType()}");
                        continue; //todo: add support for other types
                    }

                    var scale = symbolStyle.SymbolScale;

                    var size = symbolStyle.BitmapId >= 0
                        ? symbolCache.GetSize(symbolStyle.BitmapId)
                        : new Size(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);
                    
                    var marginX = size.Width * 0.5 * resolution * scale;
                    var marginY = size.Height * 0.5 * resolution * scale;
             
                    if (feature.Geometry.Touches(point, marginX, marginY)) return true;
                }
            }
            return feature.Geometry.Touches(point);
        }
    }
}