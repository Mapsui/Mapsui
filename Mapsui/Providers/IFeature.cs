using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Styles;

namespace Mapsui.Providers
{
    public interface IFeature
    {
        IGeometry Geometry { get; set; }
        IDictionary<IStyle, object> RenderedGeometry { get; }
        ICollection<IStyle> Styles { get; }
        object this[string key] { get; set; }
        IEnumerable<string> Fields { get; }
    }
}
