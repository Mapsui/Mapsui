using Mapsui.Geometries;
using Mapsui.Providers;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Styles;

namespace Mapsui.Fetcher
{
    class FeatureFetcher
    {
        private readonly BoundingBox _extent;
        private readonly double _resolution;
        private readonly DataArrivedDelegate _dataArrived;
        private readonly IProvider _provider;
        private readonly long _timeOfRequest;

        internal delegate void DataArrivedDelegate(IEnumerable<IFeature> features, object state = null);

        public FeatureFetcher(BoundingBox extent, double resolution, IProvider provider, DataArrivedDelegate dataArrived, long timeOfRequest = default(long))
        {
            _dataArrived = dataArrived;
            var biggerBox = extent.Grow(SymbolStyle.DefaultWidth * 2 * resolution, SymbolStyle.DefaultHeight * 2 * resolution);
            _extent = biggerBox;
            _provider = provider;
            _resolution = resolution;
            _timeOfRequest = timeOfRequest;
        }

        public void FetchOnThread()
        {
            lock (_provider)
            {
                var features = _provider.GetFeaturesInView(_extent, _resolution).ToList();
                _dataArrived?.Invoke(features, _timeOfRequest);
            }
        }
    }
}
