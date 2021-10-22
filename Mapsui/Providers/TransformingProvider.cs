using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Fetcher;
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

        public IEnumerable<IGeometryFeature> GetFeatures(FetchInfo fetchInfo)
        {
            fetchInfo = new FetchInfo(fetchInfo); // Copy so we do not modify the original
            fetchInfo.Extent = ProjectionHelper.Transform(fetchInfo.Extent.ToBoundingBox(), _geometryTransformation, CRS, _provider.CRS).ToMRect();
            var features = _provider.GetFeatures(fetchInfo);
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
