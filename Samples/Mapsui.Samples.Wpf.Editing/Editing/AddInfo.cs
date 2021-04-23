using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Samples.Wpf.Editing.Editing
{
    public class AddInfo
    {
        public IGeometryFeature Feature;
        public IList<Point> Vertices;
        public Point Vertex;
    }
}