using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Nts;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidget;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public class MapRenderer : IRenderer
    {
        private const int TilesToKeepMultiplier = 3;
        private const int MinimumTilesToKeep = 32;
        private readonly SymbolCache _symbolCache = new SymbolCache();
        private readonly IDictionary<object, BitmapInfo?> _tileCache =
            new Dictionary<object, BitmapInfo?>(new IdentityComparer<object>());
        private long _currentIteration;

        public ISymbolCache SymbolCache => _symbolCache;

        public IDictionary<Type, IWidgetRenderer> WidgetRenders { get; } = new Dictionary<Type, IWidgetRenderer>();

        /// <summary>
        /// Dictionary holding all special renderers for styles
        /// </summary>
        public IDictionary<Type, IStyleRenderer> StyleRenderers { get; } = new Dictionary<Type, IStyleRenderer>();

        static MapRenderer()
        {
            DefaultRendererFactory.Create = () => new MapRenderer();
        }

        public MapRenderer()
        {
            WidgetRenders[typeof(Hyperlink)] = new HyperlinkWidgetRenderer();
            WidgetRenders[typeof(ScaleBarWidget)] = new ScaleBarWidgetRenderer();
            WidgetRenders[typeof(ZoomInOutWidget)] = new ZoomInOutWidgetRenderer();
            WidgetRenders[typeof(ButtonWidget)] = new ButtonWidgetRenderer();
        }

        public void Render(object target, IReadOnlyViewport viewport, IEnumerable<ILayer> layers,
            IEnumerable<IWidget> widgets, Color? background = null)
        {
            var attributions = layers.Where(l => l.Enabled).Select(l => l.Attribution).Where(w => w != null).ToList();

            var allWidgets = widgets.Concat(attributions);

            RenderTypeSave((SKCanvas)target, viewport, layers, allWidgets, background);
        }

        private void RenderTypeSave(SKCanvas canvas, IReadOnlyViewport viewport, IEnumerable<ILayer> layers,
            IEnumerable<IWidget> widgets, Color? background = null)
        {
            if (!viewport.HasSize) return;

            if (background is not null) canvas.Clear(background.ToSkia());
            Render(canvas, viewport, layers);
            Render(canvas, viewport, widgets, 1);
        }

        public MemoryStream? RenderToBitmapStream(IReadOnlyViewport? viewport, IEnumerable<ILayer> layers, Color? background = null, float pixelDensity = 1)
        {
            if (viewport == null)
                return null;

            try
            {
                var width = viewport.Width;
                var height = viewport.Height;

                var imageInfo = new SKImageInfo((int)Math.Round(width * pixelDensity), (int)Math.Round(height * pixelDensity),
                    SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);

                using var surface = SKSurface.Create(imageInfo);
                if (surface == null) return null;
                // Not sure if this is needed here:
                if (background is not null) surface.Canvas.Clear(background.ToSkia());
                surface.Canvas.Scale(pixelDensity, pixelDensity);
                Render(surface.Canvas, viewport, layers);
                using var image = surface.Snapshot();
                using var data = image.Encode();
                var memoryStream = new MemoryStream();
                data.SaveTo(memoryStream);
                return memoryStream;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message);
                return null;
            }
        }

        private void Render(SKCanvas canvas, IReadOnlyViewport viewport, IEnumerable<ILayer> layers)
        {
            try
            {
                layers = layers.ToList();

                VisibleFeatureIterator.IterateLayers(viewport, layers, (v, l, s, f, o) => { RenderFeature(canvas, v, l, s, f, o); });

                RemovedUnusedBitmapsFromCache();

                _currentIteration++;
            }
            catch (Exception exception)
            {
                Logger.Log(LogLevel.Error, "Unexpected error in skia renderer", exception);
            }
        }

        private void RemovedUnusedBitmapsFromCache()
        {
            var tilesUsedInCurrentIteration =
                _tileCache.Values.Count(i => i?.IterationUsed == _currentIteration);
            var tilesToKeep = tilesUsedInCurrentIteration * TilesToKeepMultiplier;
            tilesToKeep = Math.Max(tilesToKeep, MinimumTilesToKeep);
            var tilesToRemove = _tileCache.Keys.Count - tilesToKeep;

            if (tilesToRemove > 0) RemoveOldBitmaps(_tileCache, tilesToRemove);
        }

        private static void RemoveOldBitmaps(IDictionary<object, BitmapInfo?> tileCache, int numberToRemove)
        {
            var counter = 0;
            var orderedKeys = tileCache.OrderBy(kvp => kvp.Value?.IterationUsed).Select(kvp => kvp.Key).ToList();
            foreach (var key in orderedKeys)
            {
                if (counter >= numberToRemove) break;
                var textureInfo = tileCache[key];
                tileCache.Remove(key);
                textureInfo?.Bitmap?.Dispose();
                counter++;
            }
        }

        private void RenderFeature(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IStyle style, IFeature feature, float layerOpacity)
        {
            // Check, if we have a special renderer for this style
            if (StyleRenderers.ContainsKey(style.GetType()))
            {
                // Save canvas
                canvas.Save();
                // We have a special renderer, so try, if it could draw this
                var styleRenderer = (ISkiaStyleRenderer)StyleRenderers[style.GetType()];
                var result = styleRenderer.Draw(canvas, viewport, layer, feature, style, _symbolCache);
                // Restore old canvas
                canvas.Restore();
                // Was it drawn?
                if (result)
                    // Yes, special style renderer drawn correct
                    return;
            }

            // No special style renderer handled this up to now, than try standard renderers
            if (feature is GeometryFeature geometryFeatureNts)
                GeometryRenderer.Draw(canvas, viewport, style, layerOpacity, geometryFeatureNts, _symbolCache);
            if (feature is PointFeature pointFeature)
                PointRenderer.Draw(canvas, viewport, style, pointFeature, pointFeature.Point.X, pointFeature.Point.Y, _symbolCache, layerOpacity * style.Opacity);
            else if (feature is RectFeature rectFeature)
                RectRenderer.Draw(canvas, viewport, style, rectFeature, layerOpacity * style.Opacity);
            else if (feature is RasterFeature rasterFeature)
                RasterRenderer.Draw(canvas, viewport, style, rasterFeature, rasterFeature.Raster, layerOpacity * style.Opacity, _tileCache, _currentIteration);

        }

        private void Render(object canvas, IReadOnlyViewport viewport, IEnumerable<IWidget> widgets, float layerOpacity)
        {
            WidgetRenderer.Render(canvas, viewport, widgets, WidgetRenders, layerOpacity);
        }

        public MapInfo? GetMapInfo(double x, double y, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, int margin = 0)
        {
            // todo: use margin to increase the pixel area
            // todo: We will need to select on style instead of layer

            layers = layers
                .Select(l => (l is ISourceLayer sl) ? sl.SourceLayer : l)
                .Where(l => l.IsMapInfoLayer);

            var list = new List<MapInfoRecord>();
            var result = new MapInfo
            {
                ScreenPosition = new MPoint(x, y),
                WorldPosition = viewport.ScreenToWorld(x, y),
                Resolution = viewport.Resolution
            };

            if (!viewport.Extent?.Contains(viewport.ScreenToWorld(result.ScreenPosition)) ?? false) return result;

            try
            {
                var width = (int)viewport.Width;
                var height = (int)viewport.Height;

                var imageInfo = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);

                var intX = (int)x;
                var intY = (int)y;

                if (intX >= width || intY >= height)
                    return result;

                using (var surface = SKSurface.Create(imageInfo))
                {
                    if (surface == null) return null;

                    surface.Canvas.ClipRect(new SKRect((float)(x - 1), (float)(y - 1), (float)(x + 1), (float)(y + 1)));
                    surface.Canvas.Clear(SKColors.Transparent);

                    using var pixmap = surface.PeekPixels();
                    var color = pixmap.GetPixelColor(intX, intY);


                    VisibleFeatureIterator.IterateLayers(viewport, layers, (v, layer, style, feature, opacity) => {
                        // ReSharper disable AccessToDisposedClosure // There is no delayed fetch. After IterateLayers returns all is done. I do not see a problem.
                        surface.Canvas.Save();
                        // 1) Clear the entire bitmap
                        surface.Canvas.Clear(SKColors.Transparent);
                        // 2) Render the feature to the clean canvas
                        RenderFeature(surface.Canvas, v, layer, style, feature, opacity);
                        // 3) Check if the pixel has changed.
                        if (color != pixmap.GetPixelColor(intX, intY))
                            // 4) Add feature and style to result
                            list.Add(new MapInfoRecord(feature, style, layer));
                        surface.Canvas.Restore();
                        // ReSharper restore AccessToDisposedClosure
                    });
                }

                if (list.Count == 0)
                    return result;

                list.Reverse();
                var itemDrawnOnTop = list.First();

                result.Feature = itemDrawnOnTop.Feature;
                result.Style = itemDrawnOnTop.Style;
                result.Layer = itemDrawnOnTop.Layer;
                result.MapInfoRecords = list;

            }
            catch (Exception exception)
            {
                Logger.Log(LogLevel.Error, "Unexpected error in skia renderer", exception);
            }

            return result;
        }

        public class IdentityComparer<T> : IEqualityComparer<T> where T : class
        {
            public bool Equals(T obj, T otherObj)
            {
                return obj == otherObj;
            }

            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}