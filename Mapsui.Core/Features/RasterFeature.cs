
namespace Mapsui.Layers
{
    public class RasterFeature : BaseFeature, IFeature
    {
        public MRaster Raster { get; set; } = default!;
        public MRect Extent => Raster;
    }
}
