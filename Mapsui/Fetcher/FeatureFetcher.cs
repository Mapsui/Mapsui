using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Fetcher
{
    internal class FeatureFetcher
    {
        private readonly FetchInfo _fetchInfo;
        private readonly DataArrivedDelegate _dataArrived;
        private readonly IProvider<IFeature> _provider;
        private readonly long _timeOfRequest;

        internal delegate void DataArrivedDelegate(IEnumerable<IFeature> features, object? state = null);

        public FeatureFetcher(FetchInfo fetchInfo, IProvider<IFeature> provider, DataArrivedDelegate dataArrived, long timeOfRequest = default)
        {
            _dataArrived = dataArrived;
            _fetchInfo = fetchInfo;
            var biggerBox = _fetchInfo.Extent.Grow(SymbolStyle.DefaultWidth * 2 * fetchInfo.Resolution, SymbolStyle.DefaultHeight * 2 * fetchInfo.Resolution);
            _fetchInfo.Extent = biggerBox;
            _provider = provider;
            _timeOfRequest = timeOfRequest;
        }

        public void FetchOnThread()
        {
            lock (_provider)
            {
                var features = _provider.GetFeatures(_fetchInfo).ToList();
                _dataArrived?.Invoke(features, _timeOfRequest);
            }
        }
    }
}
