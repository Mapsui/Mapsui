using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Logging;
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

        public async Task FetchOnThread()
        {
            try
            {
                Monitor.Enter(_provider);
                var features = await _provider.GetFeatures(_fetchInfo).ToListAsync();
                _dataArrived.Invoke(features, _timeOfRequest);
            }
            finally
            {
                Monitor.Exit(_provider);
            }
        }
    }
}
