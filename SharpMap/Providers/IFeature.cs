using System.Collections.Generic;
using SharpMap.Geometries;
using SharpMap.Styles;

namespace SharpMap.Providers
{
    public interface IFeature
    {
        IGeometry Geometry { get; set; }
        object RenderedGeometry { get; set; }
        ICollection<IStyle> Styles { get; }
        object this[string key] { get; set; }
        IEnumerable<string> Fields { get; }
    }
}
