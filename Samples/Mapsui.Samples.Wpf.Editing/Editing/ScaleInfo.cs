using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Providers;

namespace Mapsui.Samples.Wpf.Editing.Editing
{
    class ScaleInfo
    {
        public GeometryFeature? Feature { get; set; }
        public Point? PreviousPosition { get; set; }
        public Point? Center { get; set; }
    }
}
