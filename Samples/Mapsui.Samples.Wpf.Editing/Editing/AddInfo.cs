using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Providers;

namespace Mapsui.Samples.Wpf.Editing.Editing
{
    public class AddInfo
    {
        public GeometryFeature? Feature;
        public IList<Point>? Vertices;
        public Point? Vertex;
    }
}