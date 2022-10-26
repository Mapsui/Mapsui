using System;
using System.Collections.Generic;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache
{
    public class VectorCache : IVectorCache
    {
        private readonly Dictionary<(Pen? Pen, float Opacity), object> _paintCache = new();
        public T GetOrCreatePaint<T>(Pen? pen, float opacity, Func<Pen?, float, T> createSkPaint) where T : class
        {
            var key = (pen, opacity);
            if (!_paintCache.TryGetValue(key, out var paint))
            {
                paint = createSkPaint(pen, opacity);
                _paintCache[key] = paint;
            }

            return (T)paint;
        }
    }
}