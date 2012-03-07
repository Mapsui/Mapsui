using System.Collections.Generic;
using SharpMap.Geometries;
using SharpMap.Styles;

namespace SharpMap.Providers
{
    public interface IFeature
    {
        IGeometry Geometry { get; set; }
        object RenderedGeometry { get; set; }
        IStyle Style { get; set; }
        object this[string key] { get; set; }
        IEnumerable<string> Fields { get; }
    }
}
