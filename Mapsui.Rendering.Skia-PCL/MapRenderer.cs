using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public class MapRenderer : IRenderer
    {
        private const int TilesToKeepMultiplier = 3;
        private readonly SymbolCache _symbolCache = new SymbolCache();

        private readonly IDictionary<object, BitmapInfo> _tileCache =
            new Dictionary<object, BitmapInfo>(new IdentityComparer<object>());

        private long _currentIteration;

        public ISymbolCache SymbolCache => _symbolCache;

        static MapRenderer()
        {
            DefaultRendererFactory.Create = () => new MapRenderer();
        }

        public void Render(object target, IViewport viewport, IEnumerable<ILayer> layers, Color background = null)
        {
            Render((SKCanvas) target, viewport, layers, background);
        }

        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers, Color background = null)
        {
            try
            {
                using (
                    var bitmap = new SKBitmap((int) viewport.Width, (int) viewport.Height, SKColorType.Bgra8888,
                        SKAlphaType.Unpremul))
                {
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        
                        Render(canvas, viewport, layers, background);
                        using (var image = SKImage.FromBitmap(bitmap))
                        {
                            using (var data = image.Encode())
                            {
                                var memoryStream = new MemoryStream();
                                data.SaveTo(memoryStream);
                                //System.IO.File.WriteAllBytes($"c:/temp/{layers.First().Name}.png", memoryStream.ToArray());
                                return memoryStream;
                            }
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

        private void Render(SKCanvas canvas, IViewport viewport, IEnumerable<ILayer> layers, Color background)
        {
            if (background != null)
            {
                canvas.Clear(background.ToSkia());
            }
  
            layers = layers.ToList();

            SetAllTextureInfosToUnused();

            VisibleFeatureIterator.IterateLayers(viewport, layers, (v, l, s) => { RenderFeature(canvas, v, l, s); });

            RemoveUnusedTextureInfos();

            _currentIteration++;
        }

        private void RemoveUnusedTextureInfos()
        {
            var numberOfTilesUsedInCurrentIteration =
                _tileCache.Values.Count(i => i.IterationUsed == _currentIteration);

            var orderedKeys = _tileCache.OrderBy(kvp => kvp.Value.IterationUsed).Select(kvp => kvp.Key).ToList();

            var counter = 0;
            var tilesToKeep = orderedKeys.Count*TilesToKeepMultiplier;
            var numberToRemove = numberOfTilesUsedInCurrentIteration - tilesToKeep;
            foreach (var key in orderedKeys)
            {
                if (counter > numberToRemove)
                    break;
                var textureInfo = _tileCache[key];
                _tileCache.Remove(key);
                textureInfo.Bitmap.Dispose();
                counter++;
            }
        }

        private void SetAllTextureInfosToUnused()
        {
            foreach (var key in _tileCache.Keys.ToList())
            {
                var textureInfo = _tileCache[key];
                textureInfo.IterationUsed = _currentIteration;
                _tileCache[key] = textureInfo;
            }
        }

        private void RenderFeature(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is Point)
                PointRenderer.Draw(canvas, viewport, style, feature, feature.Geometry, _symbolCache);
            else if (feature.Geometry is MultiPoint)
                MultiPointRenderer.Draw(canvas, viewport, style, feature, feature.Geometry, _symbolCache);
            else if (feature.Geometry is LineString)
                LineStringRenderer.Draw(canvas, viewport, style, feature, feature.Geometry);
            else if (feature.Geometry is MultiLineString)
                MultiLineStringRenderer.Draw(canvas, viewport, style, feature, feature.Geometry);
            else if (feature.Geometry is Polygon)
                PolygonRenderer.Draw(canvas, viewport, style, feature, feature.Geometry);
            else if (feature.Geometry is MultiPolygon)
                MultiPolygonRenderer.Draw(canvas, viewport, style, feature, feature.Geometry);
            else if (feature.Geometry is IRaster)
                RasterRenderer.Draw(canvas, viewport, style, feature, _tileCache, _currentIteration);
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