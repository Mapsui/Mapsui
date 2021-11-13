
using System;

namespace Mapsui.Layers
{
    public class RasterFeature : BaseFeature, IFeature
    {
        public MRaster Raster { get; }
        public MRect Extent => Raster;

        public RasterFeature(RasterFeature rasterFeature) : base(rasterFeature)
        {
            Raster = new MRaster(rasterFeature.Raster);
        }

        public RasterFeature(MRaster raster)
        {
            Raster = raster;
        }

        public void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
        {
            foreach (var point in new[] { Raster.Min, Raster.Max })
                visit(point.X, point.Y, (x, y) => {
                    point.X = x;
                    point.Y = x;
                });
        }
    }
}
