using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.GeometryLayer
{
    public interface IGeometryFeature : IFeature
    {
        IDictionary<IStyle, object> RenderedGeometry { get; }
        public IGeometry? Geometry { get; set; }
    }
}
