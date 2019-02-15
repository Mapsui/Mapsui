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
    public static class MapInfoHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="layers">The layers to query for MapInfo</param>
        /// <param name="viewport">The current Viewport</param>
        /// <param name="screenPosition">The screenposition to query</param>
        /// <param name="symbolCache">The </param>
        /// <param name="margin">Margin of error in pixels. If the distance between screen position and geometry 
        /// is smaller than the margin it is seen as a hit.</param>
        /// <returns></returns>
        public static MapInfo GetMapInfo(IEnumerable<ILayer> layers, IReadOnlyViewport viewport, Point screenPosition,
            ISymbolCache symbolCache, int margin = 0)
        {
            var worldPosition = viewport.ScreenToWorld(screenPosition);
            return GetMapInfo(layers, worldPosition, screenPosition, viewport.Resolution, symbolCache, margin);
        }

        private static MapInfo GetMapInfo(IEnumerable<ILayer> layers, Point worldPosition,
            Point screenPosition, double resolution, ISymbolCache symbolCache, int margin = 0)
        {
            var reversedLayer = layers.Reverse();
            foreach (var layer in reversedLayer)
            {
                if (!layer.Enabled) continue;
                if (layer.MinVisible > resolution) continue;
                if (layer.MaxVisible < resolution) continue;

                var maxSymbolSize = 128; // This sucks. There should be a better way to determine max symbol size.
                var box = new BoundingBox(worldPosition, worldPosition);
                var grownBox = box.Grow(resolution * maxSymbolSize * 0.5);
                var features = layer.GetFeaturesInView(grownBox, resolution);

                var feature = features.LastOrDefault(f => 
                    IsTouchingTakingIntoAccountSymbolStyles(worldPosition, f, layer.Style, resolution, symbolCache, margin));

                if (feature != null)
                {
                    return new MapInfo
                    {
                        Feature = feature,
                        Layer = layer,
                        WorldPosition = worldPosition,
                        ScreenPosition = screenPosition,
                        Resolution = resolution
                    };
                }
            }

            // return MapInfoEventArgs without feature if none was found. Can be usefull to create features
            return new MapInfo
            {
                WorldPosition = worldPosition,
                ScreenPosition = screenPosition
            };
        }

        private static bool IsTouchingTakingIntoAccountSymbolStyles(Point point, IFeature feature, IStyle layerStyle, 
            double resolution, ISymbolCache symbolCache, int margin = 0)
        {
            var styles = new List<IStyle>();
            styles.AddRange(ToCollection(layerStyle));
            styles.AddRange(feature.Styles);

            var marginInWorldUnits = margin * resolution;

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

                        var box = feature.Geometry.BoundingBox;
                        box = box.Grow(marginX, marginY);
                        if (symbolStyle.SymbolOffset.IsRelative)
                            box.Offset(
                                size.Width * symbolStyle.SymbolOffset.X * factor,
                                size.Height * symbolStyle.SymbolOffset.Y * factor);
                        else
                            box.Offset(symbolStyle.SymbolOffset.X * factor, symbolStyle.SymbolOffset.Y * factor);
                        if (box.Distance(point) <= marginInWorldUnits) return true;
                    }
                    else if (localStyle is VectorStyle)
                    {
                        var marginX = SymbolStyle.DefaultWidth * 0.5 * resolution;
                        var marginY = SymbolStyle.DefaultHeight * 0.5 * resolution;

                        var box = feature.Geometry.BoundingBox;
                        box = box.Grow(marginX, marginY);
                        if (box.Distance(point) <= marginInWorldUnits) return true;
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
                        var lineWidthInWorldUnits = symbolStyle.Line.Width * resolution * 0.5;

                        if (feature.Geometry.Distance(point) <= lineWidthInWorldUnits + marginInWorldUnits) return true;
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warning, $"Feature info not supported for lines with {localStyle.GetType()}");
                    }

                }
            }
            else
            {
                return feature.Geometry.Distance(point) <= marginInWorldUnits;
            }
            return false;
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