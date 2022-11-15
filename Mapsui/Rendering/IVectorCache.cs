using System;
using Mapsui.Styles;

namespace Mapsui.Rendering
{
    public interface IVectorCache
    {
        T GetOrCreatePaint<T>(Pen? pen, float opacity, Func<Pen?, float, T> toPaint)
            where T : class;

        TPath GetOrCreatePath<TPath, TGeometry>(IReadOnlyViewport viewport, TGeometry geometry, Func<TGeometry, IReadOnlyViewport, TPath> toPath)
            where TPath : class
            where TGeometry : class;
    }
}