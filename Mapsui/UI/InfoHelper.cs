using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;

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
            var styles = new List<IStyle>();
            styles.AddRange(ToCollection(layerStyle));
            styles.AddRange(feature.Styles);
            
            if (feature.Geometry is Point)
            {
                foreach (var style in styles)
                {
                    var localStyle = HandleThemeStyle(feature, style);

                    if (localStyle is SymbolStyle symbolStyle)
                    {
                        var scale = symbolStyle.SymbolScale;

                        var size = symbolStyle.BitmapId >= 0
                            ? symbolCache.GetSize(symbolStyle.BitmapId)
                            : new Size(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);

                        // Symbols allways drawn around the center (* 0.5 instead of / 2)
                        var factor = resolution * scale;
                        var marginX = size.Width * 0.5 * factor;
                        var marginY = size.Height * 0.5 * factor;

                        var box = feature.Geometry.GetBoundingBox();
                        box = box.Grow(marginX, marginY);
                        if (symbolStyle.SymbolOffset.IsRelative)
                            box.Offset(
                                size.Width * symbolStyle.SymbolOffset.X * factor,
                                size.Height * symbolStyle.SymbolOffset.Y * factor);
                        else
                            box.Offset(symbolStyle.SymbolOffset.X * factor, symbolStyle.SymbolOffset.Y * factor);
                        if (box.Contains(point)) return true;
                    }
                    else if (localStyle is VectorStyle)
                    {
                        var marginX = SymbolStyle.DefaultWidth * 0.5 * resolution;
                        var marginY = SymbolStyle.DefaultHeight * 0.5 * resolution;

                        var box = feature.Geometry.GetBoundingBox();
                        box = box.Grow(marginX, marginY);
                        if (box.Contains(point)) return true;
                    }
                    else
                    {
                        if (!(localStyle is LabelStyle)) // I don't intend to support label click, so don't warn
                        {
                            Logger.Log(LogLevel.Warning, $"Feature info not supported for points with {localStyle.GetType()}");
                        }
                    }
                }
            }
            else if (feature.Geometry is LineString || feature.Geometry is MultiLineString)
            {
                foreach (var style in styles)
                {
                    var localStyle = HandleThemeStyle(feature, style);

                    if (localStyle is VectorStyle symbolStyle)
                    {
                        var screenDistance = symbolStyle.Line.Width * resolution * 0.5;

                        if (screenDistance > feature.Geometry.Distance(point)) return true;
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warning, $"Feature info not supported for lines with {localStyle.GetType()}");
                    }

                }
            }
            return feature.Geometry.Contains(point);
        }

        private static IStyle HandleThemeStyle(IFeature feature, IStyle style)
        {
            if (style is IThemeStyle themeStyle) return themeStyle.GetStyle(feature);
            return style;
        }

        private static ICollection<IStyle> ToCollection(IStyle style)
        {
            if (style == null) return new List<IStyle>();
            return (style as StyleCollection)?.ToArray() ?? new[] { style };
        }
    }
}