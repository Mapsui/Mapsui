
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

        public PictureFeature(object? picture, MRect? extent = null)
        {
            Picture = picture;
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
#pragma warning disable IDISP007 // Don't dispose injected
            (this.Picture as IDisposable)?.Dispose();
#pragma warning restore IDISP007 // Don't dispose injected
            this.Picture = null;
        }
    }
}
