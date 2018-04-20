using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Samples.Wpf.Editing.Editing
{
    public class DragInfo
    {
        public IFeature Feature { get; set; }
        public Point Vertex { get; set; }
        public Point StartOffsetToVertex { get; set; }
    }
}