using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Fetcher;

internal class FeatureFetchDispatcher(ConcurrentStack<IFeature> cache)
{
    private FetchInfo? _fetchInfo;
    private readonly ConcurrentStack<IFeature> _cache = cache;
    private bool _modified;

    public bool TryTake([NotNullWhen(true)] out Func<Task>? method)
    {
        method = null;
        if (!_modified) return false;
        if (_fetchInfo == null) return false;

        method = async () => await FetchAsync(new FetchInfo(_fetchInfo)).ConfigureAwait(false);
        _modified = false;
        return true;
    }

    public async Task FetchAsync()
    {
        if (_fetchInfo == null) return;

        await FetchAsync(new FetchInfo(_fetchInfo)).ConfigureAwait(false);
    }

    public async Task FetchAsync(FetchInfo fetchInfo)
    {
        try
        {
            var features = DataSource != null ? await DataSource.GetFeaturesAsync(fetchInfo).ConfigureAwait(false) : [];

            FetchCompleted(features, null);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
            FetchCompleted(null, ex);
        }
    }

    private void FetchCompleted(IEnumerable<IFeature>? features, Exception? exception)
    {
        if (exception == null)
        {
            _cache.Clear();
            if (features?.Any() ?? false)
                _cache.PushRange(features.ToArray());
        }


        DataChanged?.Invoke(this, new DataChangedEventArgs(exception, false, null));
    }

    public void SetViewport(FetchInfo fetchInfo)
    {
        // Fetch a bigger extent to include partially visible symbols. 
        // todo: Take into account the maximum symbol size of the layer
        _fetchInfo = fetchInfo.Grow(SymbolStyle.DefaultWidth);
        _modified = true;
    }

    public IProvider? DataSource { get; set; }


    public event DataChangedEventHandler? DataChanged;
}
