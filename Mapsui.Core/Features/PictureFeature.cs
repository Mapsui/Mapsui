
using System;

namespace Mapsui.Layers
{
    public class PictureFeature : BaseFeature, IFeature
    {
        public object? Picture { get; private set; }
        public MRect? Extent { get; }

        public PictureFeature(PictureFeature rasterFeature) : base(rasterFeature)
        {
            Picture = rasterFeature.Picture;
            Extent = rasterFeature.Extent;
        }

        public PictureFeature(object? raster, MRect? extent = null)
        {
            Picture = raster;
            Extent = extent;
        }

        public void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
        {
            if (Extent != null)
                foreach (var point in new[] { Extent.Min, Extent.Max })
                    visit(point.X, point.Y, (x, y) => {
                        point.X = x;
                        point.Y = x;
                    });
        }

        public override void Dispose()
        {
            (this.Picture as IDisposable)?.Dispose();
            this.Picture = null;
        }
    }
}
