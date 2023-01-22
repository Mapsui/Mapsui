using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Projections;

public interface IProjection
{
    (double X, double Y) Project(string fromCRS, string toCRS, double x, double y);
    void Project(string fromCRS, string toCRS, MPoint point);
    void Project(string fromCRS, string toCRS, MRect rect);
    bool IsProjectionSupported([NotNullWhen(true)] string? fromCRS, [NotNullWhen(true)] string? toCRS);
    void Project(string fromCRS, string toCRS, IFeature feature);
    void Project(string fromCRS, string toCRS, IEnumerable<IFeature> features);
}
