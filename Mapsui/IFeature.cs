using System;
using System.Collections.Generic;
using Mapsui.Styles;

namespace Mapsui;

public delegate void CoordinateSetter(double x, double y);

public interface IFeature : IDisposable
{
    ICollection<IStyle> Styles { get; }
    object? this[string key] { get; set; }
    IEnumerable<string> Fields { get; }
    MRect? Extent { get; }
    public IDictionary<IStyle, object> RenderedGeometry { get; }
    void CoordinateVisitor(Action<double, double, CoordinateSetter> visit);
}
