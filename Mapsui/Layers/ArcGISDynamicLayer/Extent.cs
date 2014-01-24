using Mapsui.ArcGISDynamicLayer;

namespace Mapsui.Layers.ArcGISDynamicLayer
{
    public class Extent
    {
        public double xmin { get; set; }
        public double ymin { get; set; }
        public double xmax { get; set; }
        public double ymax { get; set; }
        public SpatialReference spatialReference { get; set; }
    }
}
