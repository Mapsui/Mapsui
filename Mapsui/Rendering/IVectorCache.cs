using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IVectorCache
{
    T GetOrCreatePaint<T>(Pen? pen, float opacity, Func<Pen?, float, T> toPaint)
        where T : class;

    T GetOrCreatePaint<T>(Brush? brush, float opacity, double rotation, Func<Brush?, float, double, ISymbolCache, T> toPaint)
        where T : class;

    TPath GetOrCreatePath<TPath, TGeometry>(IReadOnlyViewport viewport, TGeometry geometry, float lineWidth, Func<TGeometry, IReadOnlyViewport, float, TPath> toPath)
        where TPath : class
        where TGeometry : class;
}
