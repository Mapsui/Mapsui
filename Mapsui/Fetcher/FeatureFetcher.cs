using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Fetcher;

internal class FeatureFetcher
{
    private readonly FetchInfo _fetchInfo;
    private readonly DataArrivedDelegate _dataArrived;
    private readonly IProvider _provider;
    private readonly AsyncLock _providerLock = new();
    private readonly long _timeOfRequest;

    public delegate void DataArrivedDelegate(IEnumerable<IFeature> features, object? state = null);

    public FeatureFetcher(FetchInfo fetchInfo, IProvider provider, DataArrivedDelegate dataArrived, long timeOfRequest = default)
    {
        _dataArrived = dataArrived;
        _fetchInfo = fetchInfo.Grow(SymbolStyle.DefaultWidth);
        _provider = provider;
        _timeOfRequest = timeOfRequest;
    }

    public async Task FetchOnThreadAsync()
    {
        using (await _providerLock.LockAsync())
        {
            var features = await _provider.GetFeaturesAsync(_fetchInfo).ConfigureAwait(false);
            _dataArrived.Invoke(features, _timeOfRequest);
        }
    }
}
