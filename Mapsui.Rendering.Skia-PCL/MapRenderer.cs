using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public class MapRenderer : IRenderer
    {
        private const int TilesToKeepMultiplier = 3;
        private const int MinimumTilesToKeep = 32;
        private readonly SymbolCache _symbolCache = new SymbolCache();

        private readonly IDictionary<object, BitmapInfo> _tileCache =
            new Dictionary<object, BitmapInfo>(new IdentityComparer<object>());

        private long _currentIteration;

        public ISymbolCache SymbolCache => _symbolCache;

        static MapRenderer()
        {
            DefaultRendererFactory.Create = () => new MapRenderer();
        }
        public void Render(object target, IViewport viewport, IEnumerable<ILayer> layers,
            IEnumerable<IWidget> widgets, Color background = null)
        {
            var allWidgets = layers.Select(l => l.Attribution).ToList().Concat(widgets);
            Render((SKCanvas)target, viewport, layers, allWidgets, background);
        }

        private void Render(SKCanvas canvas, IViewport viewport, IEnumerable<ILayer> layers,
            IEnumerable<IWidget> widgets, Color background = null)
        {
            if (background != null) canvas.Clear(background.ToSkia(1));
            if (viewport.Initialized) Render(canvas, viewport, layers);
            Render(canvas, viewport, widgets, 1);
        }

        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers, Color background = null)
        {
            try
            {
                using (var surface = SKSurface.Create(
                    (int)viewport.Width, (int)viewport.Height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul))
                {
                    // Not sure if this is needed here:
                    if (background != null) surface.Canvas.Clear(background.ToSkia(1));
                    Render(surface.Canvas, viewport, layers);
                    using (var image = surface.Snapshot())
                    {
                        using (var data = image.Encode())
                        {
                            var memoryStream = new MemoryStream();
                            data.SaveTo(memoryStream);
                            return memoryStream;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message);
                return null;
            }
        }

        private void Render(SKCanvas canvas, IViewport viewport, IEnumerable<ILayer> layers)
        {
            try
            {
                layers = layers.ToList();

                VisibleFeatureIterator.IterateLayers(viewport, layers, (v, l, s, o) => { RenderFeature(canvas, v, l, s, o); });

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
                _tileCache.Values.Count(i => i.IterationUsed == _currentIteration);
            var tilesToKeep = tilesUsedInCurrentIteration * TilesToKeepMultiplier;
            tilesToKeep = Math.Max(tilesToKeep, MinimumTilesToKeep);
            var tilesToRemove = _tileCache.Keys.Count - tilesToKeep;

            if (tilesToRemove > 0) RemoveOldBitmaps(_tileCache, tilesToRemove);
        }

        private static void RemoveOldBitmaps(IDictionary<object, BitmapInfo> tileCache, int numberToRemove)
        {
            var counter = 0;
            var orderedKeys = tileCache.OrderBy(kvp => kvp.Value.IterationUsed).Select(kvp => kvp.Key).ToList();
            foreach (var key in orderedKeys)
            {
                if (counter >= numberToRemove) break;
                var textureInfo = tileCache[key];
                tileCache.Remove(key);
                textureInfo.Bitmap.Dispose();
                counter++;
            }
        }

        private void RenderFeature(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature, float layerOpacity)
        {
            if (feature.Geometry is Point)
                PointRenderer.Draw(canvas, viewport, style, feature, feature.Geometry, _symbolCache, layerOpacity * style.Opacity);
            else if (feature.Geometry is MultiPoint)
                MultiPointRenderer.Draw(canvas, viewport, style, feature, feature.Geometry, _symbolCache, layerOpacity * style.Opacity);
            else if (feature.Geometry is LineString)
                LineStringRenderer.Draw(canvas, viewport, style, feature, feature.Geometry, layerOpacity * style.Opacity);
            else if (feature.Geometry is MultiLineString)
                MultiLineStringRenderer.Draw(canvas, viewport, style, feature, feature.Geometry, layerOpacity * style.Opacity);
            else if (feature.Geometry is Polygon)
                PolygonRenderer.Draw(canvas, viewport, style, feature, feature.Geometry, layerOpacity * style.Opacity, _symbolCache);
            else if (feature.Geometry is MultiPolygon)
                MultiPolygonRenderer.Draw(canvas, viewport, style, feature, feature.Geometry, layerOpacity * style.Opacity, _symbolCache);
            else if (feature.Geometry is IRaster)
                RasterRenderer.Draw(canvas, viewport, style, feature, layerOpacity * style.Opacity, _tileCache, _currentIteration);
        }

        private void Render(object canvas, IViewport viewport, IEnumerable<IWidget> widgets, float layerOpacity)
        {
            WidgetRenderer.Render(canvas, viewport.Width, viewport.Height, widgets, layerOpacity);
        }
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