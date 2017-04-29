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
        public static InfoEventArgs GetInfoEventArgs(IViewport viewport, Point screenPosition, IEnumerable<ILayer> layers,
            ISymbolCache symbolCache)
        {
            var worldPosition = viewport.ScreenToWorld(new Point(screenPosition.X, screenPosition.Y));
            return GetInfoEventArgs(layers, worldPosition, screenPosition, viewport.Resolution, symbolCache);
        }

        private static InfoEventArgs GetInfoEventArgs(IEnumerable<ILayer> layers, Point worldPosition, Point screenPosition,
            double resolution, ISymbolCache symbolCache)
        {
            foreach (var layer in layers)
            {
                if (layer.Enabled == false) continue;
                if (layer.MinVisible > resolution) continue;
                if (layer.MaxVisible < resolution) continue;
                
                var allFeatures = layer.GetFeaturesInView(layer.Envelope, resolution);
                
                var features = allFeatures.Where(f => 
                    IsTouchingTakingIntoAccountSymbolStyles(worldPosition, f, layer.Style, resolution, symbolCache)).ToList();

                var feature = features.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Distance(worldPosition))
                    .FirstOrDefault();
                
                if (feature != null)
                {
                    return new InfoEventArgs
                    {
                        Feature = feature,
                        Layer = layer,
                        WorldPosition = worldPosition,
                        ScreenPosition = screenPosition
                    };
                }
            }
            return new InfoEventArgs { WorldPosition = worldPosition};
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
                        Logger.Log(LogLevel.Warning, $"Feature info not supported for {style.GetType()}");
                        continue; //todo: add support for other types
                    }

                    var scale = symbolStyle.SymbolScale;

                    var size = symbolStyle.BitmapId >= 0
                        ? symbolCache.GetSize(symbolStyle.BitmapId)
                        : new Size(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);

                    var factor = resolution * scale;
                    var marginX = size.Width * 0.5 * factor;
                    var marginY = size.Height * 0.5 * factor;

                    var box = feature.Geometry.GetBoundingBox();
                    box = box.Grow(marginX, marginY);
                    box.Offset(symbolStyle.SymbolOffset.X * factor, symbolStyle.SymbolOffset.Y * factor);
                    if (box.Contains(point)) return true;
                }
            }
            return feature.Geometry.Contains(point);
        }
    }
}