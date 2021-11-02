using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Providers
{
    public interface IGeometryFeature : IFeature
    {
        public IGeometry? Geometry { get; set; }
        IDictionary<IStyle, object> RenderedGeometry { get; }
    }
}
