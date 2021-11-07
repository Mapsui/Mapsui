
namespace Mapsui.Layers
{
    public class RasterFeature : BaseFeature, IFeature
    {
        public MRaster Raster { get; set; }
        public MRect Extent => Raster;
    }
}
