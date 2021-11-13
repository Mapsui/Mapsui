using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Utilities;

namespace Mapsui.Providers
{
    public class ProjectingProvider : IProvider<GeometryFeature>
    {
        private readonly IProvider<GeometryFeature> _provider;
        private readonly IProjection _geometryProjection;

        public ProjectingProvider(IProvider<GeometryFeature> provider, IProjection? geometryProjection = null)
        {
            _provider = provider;
            _geometryProjection = geometryProjection ?? new MinimalProjection();
        }

        public string CRS { get; set; }

        public IEnumerable<GeometryFeature> GetFeatures(FetchInfo fetchInfo)
        {
            var projectedExtent = ProjectionHelper.Project(fetchInfo.Extent, _geometryProjection, CRS, _provider.CRS);
            if (projectedExtent == null) return new List<GeometryFeature>(); // Perhaps Project should not return null
            fetchInfo = new FetchInfo(projectedExtent, fetchInfo.Resolution, fetchInfo.CRS, fetchInfo.ChangeType);

            var features = _provider.GetFeatures(fetchInfo);
            return ProjectionHelper.Project(features, _geometryProjection, _provider.CRS, CRS);
        }

        public MRect GetExtent()
        {
            // This projects the full extent of the source. Usually the full extent of the source does not change,
            // so perhaps this should be calculated just once. Then again, there are probably situations where it does
            // change so a way to refresh this should be possible.
            return ProjectionHelper.Project(_provider.GetExtent(), _geometryProjection, _provider.CRS, CRS);
        }
    }
}
