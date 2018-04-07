using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Samples.Wpf.Editing.Editing
{
    public class RotateInfo
    {
        public IFeature Feature { get; set; }
        public Point PreviousPosition { get; set; }
        public Point Center { get; set; }
    }
}
