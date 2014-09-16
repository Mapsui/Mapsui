using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Graphics.ES11;

namespace Mapsui.Rendering.OpenTK
{
    public class MapRenderer : IRenderer
    {
        readonly IDictionary<int, TextureInfo> _symbolTextureCache = new Dictionary<int, TextureInfo>();
        readonly IDictionary<object, TextureInfo> _tileTextureCache = new Dictionary<object, TextureInfo>(new IdentityComparer<object>());

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            layers = layers.ToList();

            SetAllTextureInfosToUnused();

            VisibleFeatureIterator.IterateLayers(viewport, layers, RenderFeature);

            RemoveUnusedTextureInfos();
        }

        private void RemoveUnusedTextureInfos()
        {
            foreach (var key in _tileTextureCache.Keys.ToList())
            {
                if (_tileTextureCache[key].Used == false)
                {
                    var textureInfo = _tileTextureCache[key];
                    _tileTextureCache.Remove(key);
                    GL.BindTexture(All.Texture2D, 0);
                    GL.DeleteTextures(1, ref textureInfo.TextureId);
                }
            }
        }

        private void SetAllTextureInfosToUnused()
        {
            foreach (var key in _tileTextureCache.Keys.ToList())
            {
                var textureInfo = _tileTextureCache[key];
                textureInfo.Used = false;
                _tileTextureCache[key] = textureInfo;
            }
        }

        private void RenderFeature(IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is Point) 
            {
                PointRenderer.Draw(viewport, style, feature, _symbolTextureCache);
            }
            else if (feature.Geometry is LineString) 
            {
                LineStringRenderer.Draw(viewport, style, feature);
            }
            else if (feature.Geometry is IRaster) 
            {
                RasterRenderer.Draw(viewport, style, feature, _tileTextureCache);
            }
        }

        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers)
        {
            throw new NotImplementedException();
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
