using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Providers;

namespace Mapsui.Samples.Wpf.Editing.Editing
{
    public class DragInfo
    {
        public GeometryFeature? Feature { get; set; }
        public Point? Vertex { get; set; }
        public Point? StartOffsetToVertex { get; set; }
    }
}