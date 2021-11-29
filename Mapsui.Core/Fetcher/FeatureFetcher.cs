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

        public delegate void DataArrivedDelegate(IEnumerable<IFeature> features, object? state = null);

        public FeatureFetcher(FetchInfo fetchInfo, IProvider<IFeature> provider, DataArrivedDelegate dataArrived, long timeOfRequest = default)
        {
            _dataArrived = dataArrived;
            var biggerBox = fetchInfo.Extent.Grow(
                SymbolStyle.DefaultWidth * 2 * fetchInfo.Resolution,
                SymbolStyle.DefaultHeight * 2 * fetchInfo.Resolution);
            _fetchInfo = new FetchInfo(biggerBox, fetchInfo.Resolution, fetchInfo.CRS, fetchInfo.ChangeType);

            _provider = provider;
            _timeOfRequest = timeOfRequest;
        }

        public void FetchOnThread()
        {
            lock (_provider)
            {
                var features = _provider.GetFeatures(_fetchInfo)?.ToList() ?? new List<IFeature>();
                _dataArrived.Invoke(features, _timeOfRequest);
            }
        }
    }
}
