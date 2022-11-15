using System;
using System.Collections.Generic;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache
{
    public class VectorCache : IVectorCache
    {
        private readonly Dictionary<(Pen? Pen, float Opacity), object> _paintCache = new();
        private readonly Dictionary<(MRect Rect, double Resolution, object Geometry), object> _pathCache = new();
        
        public T GetOrCreatePaint<T>(Pen? pen, float opacity, Func<Pen?, float, T> toPaint) where T : class
        {
            var key = (pen, opacity);
            if (!_paintCache.TryGetValue(key, out var paint))
            {
                paint = toPaint(pen, opacity);
                _paintCache[key] = paint;
            }

            return (T)paint;
        }

        public TPath GetOrCreatePath<TPath, TGeometry>(IReadOnlyViewport viewport, TGeometry geometry, Func<TGeometry, IReadOnlyViewport, TPath> toPath) where TPath : class where TGeometry : class
        {
            var key = (viewport.Extent, viewport.Rotation, geometry);
            if (!_pathCache.TryGetValue(key, out var path))
            {
                path = toPath(geometry, viewport);
                _pathCache[key] = path;
            }

            return (TPath)path;
        }
    }
}