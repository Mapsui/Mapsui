using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Samples.Wpf.Editing.Editing
{
    public class DragInfo
    {
        public IGeometryFeature Feature { get; set; }
        public Point Vertex { get; set; }
        public Point StartOffsetToVertex { get; set; }
    }
}