
namespace Mapsui.Layers
{
    public class RasterFeature : BaseFeature, IFeature
    {
        public MRaster Raster { get; set; } = MRaster.Empty;
        public MRect Extent => Raster;
    }
}
