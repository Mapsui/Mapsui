using System;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Fetcher;

internal class FeatureFetchDispatcher()
{
    private FetchInfo? _fetchInfo;

    public IProvider? DataSource { get; set; }

    public async Task FetchAsync(Action<FetchResult> getResult)
    {
        if (_fetchInfo == null)
            return;

        try
        {
            var features = DataSource != null ? await DataSource.GetFeaturesAsync(_fetchInfo).ConfigureAwait(false) : [];
            getResult(new FetchResult(features.ToArray()));
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
            getResult(new FetchResult(ex));
        }
    }

    public void SetViewport(FetchInfo fetchInfo) => _fetchInfo = fetchInfo.Grow(SymbolStyle.DefaultWidth); // Fetch a bigger extent to include partially visible symbols. Todo: Take into account the maximum symbol size of the layer
}
