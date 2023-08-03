using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IVectorCache
{
    T GetOrCreatePaint<T>(Pen? pen, float opacity, Func<Pen?, float, T> toPaint)
        where T : class;

    T GetOrCreatePaint<T>(Brush? brush, float opacity, double rotation, Func<Brush?, float, double, ISymbolCache, T> toPaint)
        where T : class;

    T GetOrCreateRect<T>(Viewport viewport, Func<Viewport, T> toSkRect);

    TPath GetOrCreatePath<TPath, TGeometry>(Viewport viewport, TGeometry geometry, float lineWidth, Func<TGeometry, Viewport, float, TPath> toPath)
        where TPath : class
        where TGeometry : class;
}
