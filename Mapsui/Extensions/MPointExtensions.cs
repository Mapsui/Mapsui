using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;

namespace Mapsui.Extensions;

public static class MPointExtensions
{
    public static IEnumerable<PointFeature> ToFeatures(this IEnumerable<MPoint> points)
    {
        return points.Select(p => new PointFeature(p)).ToList();
    }
}
