using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Utilities;

namespace Mapsui.Projection
{
    public class Transformer
    {
        public string FromCRS { get; set; }
        public string ToCrs { get; set; }
        public ITransformation Transformation { get; set; }

        public BoundingBox Transform(BoundingBox extent)
        {
            return ProjectionHelper.Transform(extent, Transformation, FromCRS, ToCrs);
        }

        public BoundingBox TransformBack(BoundingBox extent)
        {
            return ProjectionHelper.Transform(extent, Transformation, ToCrs, FromCRS);
        }

        public IEnumerable<IFeature> Transform(IEnumerable<IFeature> features)
        {
            return ProjectionHelper.Transform(features, Transformation, FromCRS, ToCrs);
        }

        public IEnumerable<IFeature> TransformBack(IEnumerable<IFeature> features)
        {
            return ProjectionHelper.Transform(features, Transformation, ToCrs, FromCRS);
        }
    }
}
