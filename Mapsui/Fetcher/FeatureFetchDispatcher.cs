﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Fetcher;

internal class FeatureFetchDispatcher<T> : IFetchDispatcher where T : IFeature
{
    private FetchInfo? _fetchInfo;
    private bool _busy;
    private readonly ConcurrentStack<IFeature> _cache;
    private bool _modified;

    public FeatureFetchDispatcher(ConcurrentStack<IFeature> cache)
    {
        _cache = cache;
    }

    public bool TryTake([NotNullWhen(true)] out Func<Task>? method)
    {
        method = null;
        if (!_modified) return false;
        if (_fetchInfo == null) return false;

        method = async () => await FetchOnThreadAsync(new FetchInfo(_fetchInfo)).ConfigureAwait(false);
        _modified = false;
        return true;
    }

    public async Task FetchOnThreadAsync(FetchInfo fetchInfo)
    {
        try
        {
            var features = DataSource != null ? await DataSource.GetFeaturesAsync(fetchInfo).ConfigureAwait(false) : new List<IFeature>();

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

        Busy = _modified;

        DataChanged?.Invoke(this, new DataChangedEventArgs(exception, false, null));
    }

    public void SetViewport(FetchInfo fetchInfo)
    {
        // Fetch a bigger extent to include partially visible symbols. 
        // todo: Take into account the maximum symbol size of the layer

        _fetchInfo = fetchInfo.Grow(SymbolStyle.DefaultWidth);


        _modified = true;
        Busy = true;
    }

    public IProvider? DataSource { get; set; }

    public bool Busy
    {
        get => _busy;
        private set
        {
            if (_busy == value) return; // prevent notify              
            _busy = value;
            OnPropertyChanged(nameof(Busy));
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event DataChangedEventHandler? DataChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
}
