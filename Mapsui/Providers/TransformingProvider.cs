using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Utilities;

namespace Mapsui.Providers
{
    public class TransformingProvider : IProvider<IGeometryFeature>
    {
        private readonly IProvider<IGeometryFeature> _provider;
        private readonly IGeometryTransformation _geometryTransformation;

        public TransformingProvider(IProvider<IGeometryFeature> provider, IGeometryTransformation? geometryTransformation = null)
        {
            _provider = provider;
            _geometryTransformation = geometryTransformation ?? new GeometryTransformation();
        }

        public string CRS { get; set; }

        public IEnumerable<IGeometryFeature> GetFeatures(FetchInfo fetchInfo)
        {
            var transformedExtent = ProjectionHelper.Transform(fetchInfo.Extent.ToBoundingBox(), _geometryTransformation, CRS, _provider.CRS).ToMRect();
            if (transformedExtent == null) return new List<IGeometryFeature>(); // Perhaps Transform should not return null
            fetchInfo = new FetchInfo(transformedExtent, fetchInfo.Resolution, fetchInfo.CRS, fetchInfo.ChangeType);

            var features = _provider.GetFeatures(fetchInfo);
            return ProjectionHelper.Transform(features, _geometryTransformation, _provider.CRS, CRS);
        }

        public MRectangle GetExtent()
        {
            // This transforms the full extent of the source. Usually the full extent of the source does not change,
            // so perhaps this should be calculated just once. Then again, there are probably situations where it does
            // change so a way to refresh this should be possible.
            return ProjectionHelper.Transform(_provider.GetExtent().ToBoundingBox(), _geometryTransformation, _provider.CRS, CRS).ToMRect();
        }
    }
}
