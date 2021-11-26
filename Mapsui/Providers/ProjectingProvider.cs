using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Utilities;

namespace Mapsui.Providers
{
    public class ProjectingProvider : IProvider<IFeature>
    {
        private readonly IProvider<IFeature> _provider;
        private readonly IProjection _projection;

        public ProjectingProvider(IProvider<IFeature> provider, IProjection? projection = null)
        {
            _provider = provider;
            _projection = projection ?? new MinimalProjection();
        }

        public string CRS { get; set; }

        public IEnumerable<IFeature> GetFeatures(FetchInfo fetchInfo)
        {
            var projectedExtent = ProjectionHelper.Project(fetchInfo.Extent, _projection, CRS, _provider.CRS);
            if (projectedExtent == null) return new List<IFeature>();
            fetchInfo = new FetchInfo(projectedExtent, fetchInfo.Resolution, fetchInfo.CRS, fetchInfo.ChangeType);

            var features = _provider.GetFeatures(fetchInfo);
            return ProjectionHelper.Project(features, _projection, _provider.CRS, CRS);
        }

        public MRect GetExtent()
        {
            // This projects the full extent of the source. Usually the full extent of the source does not change,
            // so perhaps this should be calculated just once. Then again, there are probably situations where it does
            // change so a way to refresh this should be possible.
            return ProjectionHelper.Project(_provider.GetExtent(), _projection, _provider.CRS, CRS);
        }
    }
}
