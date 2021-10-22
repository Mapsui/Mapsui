using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Projection;
using Mapsui.Utilities;

namespace Mapsui.Providers
{
    public class TransformingProvider : IProvider<IGeometryFeature>
    {
        private readonly IProvider<IGeometryFeature> _provider;
        private readonly IGeometryTransformation _geometryTransformation;

        public TransformingProvider(IProvider<IGeometryFeature> provider, IGeometryTransformation geometryTransformation = null)
        {
            _provider = provider;
            _geometryTransformation = geometryTransformation ?? new GeometryTransformation();
        }

        public string CRS { get; set; }

        public IEnumerable<IGeometryFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var transformedBoundingBox = ProjectionHelper.Transform(box, _geometryTransformation, CRS, _provider.CRS);
            var features = _provider.GetFeaturesInView(transformedBoundingBox, resolution);
            return ProjectionHelper.Transform(features, _geometryTransformation, _provider.CRS, CRS);
        }

        public BoundingBox GetExtents()
        {
            // This transforms the full extent of the source. Usually the full extent of the source does not change,
            // so perhaps this should be calculated just once. Then again, there are probably situations where it does
            // change so a way to refresh this should be possible.
            return ProjectionHelper.Transform(_provider.GetExtents(), _geometryTransformation, _provider.CRS, CRS);
        }
    }
}
