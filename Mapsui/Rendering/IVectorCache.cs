using System;
using Mapsui.Styles;

namespace Mapsui.Rendering
{
    public interface IVectorCache
    {
        T GetOrCreatePaint<T>(Pen? pen, float opacity, Func<Pen?, float, T> createSkPaint)
            where T : class;
    }
}