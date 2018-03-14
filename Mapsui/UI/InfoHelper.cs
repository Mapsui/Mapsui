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
        public static InfoEventArgs GetInfoEventArgs(IViewport viewport, Point screenPosition, 
            float scale, IEnumerable<ILayer> layers, ISymbolCache symbolCache, int numTaps)
        {
            var worldPosition = viewport.ScreenToWorld(
                new Point(screenPosition.X / scale, screenPosition.Y / scale));
            return GetInfoEventArgs(layers, worldPosition, screenPosition, viewport.Resolution, symbolCache, numTaps);
        }

        private static InfoEventArgs GetInfoEventArgs(IEnumerable<ILayer> layers, Point worldPosition, Point screenPosition,
            double resolution, ISymbolCache symbolCache, int numTaps)
        {
            var reversedLayer = layers.Reverse();
            foreach (var layer in reversedLayer)
            {
                if (layer.Enabled == false) continue;
                if (layer.MinVisible > resolution) continue;
                if (layer.MaxVisible < resolution) continue;
                
                var features = layer.GetFeaturesInView(layer.Envelope, resolution);
                
                var feature = features
                    .LastOrDefault(f => IsTouchingTakingIntoAccountSymbolStyles(worldPosition, f, layer.Style, resolution, symbolCache));
                
                if (feature != null)
                {
                    return new InfoEventArgs
                    {
                        Feature = feature,
                        Layer = layer,
                        WorldPosition = worldPosition,
                        ScreenPosition = screenPosition,
                        NumTaps = numTaps,
                        Handled = false,
                    };
                }
            }
            // return InfoEventArgs without feature if none was found. Can be usefull to create features
            return new InfoEventArgs { WorldPosition = worldPosition, ScreenPosition = screenPosition, NumTaps = numTaps, Handled = false};
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
                    if (style is SymbolStyle symbolStyle)
                    {
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
                    if (style is VectorStyle)
                    {
                        var marginX = SymbolStyle.DefaultWidth * 0.5 * resolution;
                        var marginY = SymbolStyle.DefaultHeight * 0.5 * resolution;

                        var box = feature.Geometry.GetBoundingBox();
                        box = box.Grow(marginX, marginY);
                        if (box.Contains(point)) return true;
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warning, $"Feature info not supported for {style.GetType()}");
                        //todo: add support for other types
                    }
                }
            }
            return feature.Geometry.Contains(point);
        }
    }
}