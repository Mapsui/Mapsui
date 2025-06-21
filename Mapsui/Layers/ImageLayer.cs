// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Fetcher;
using Mapsui.Logging;
using Mapsui.Manipulations;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Layers;

public class ImageLayer : BaseLayer, IAsyncDataFetcher, ILayerDataSource<IProvider>, IDisposable, ILayer, ILayerFeatureInfo
{
    private IEnumerable<IFeature> _cache = [];
    private IProvider? _dataSource;
    private int _delayBetweenCalls = 0;
    private int _refreshCounter; // To determine if fetching is still Busy. Multiple refreshes can be in progress. To know if the last one was handled we use this counter.

    public ImageLayer()
    {
        Style = new RasterStyle();
    }

    public ImageLayer(string layerName) : this()
    {
        Name = layerName;
    }

    public Delayer Delayer { get; } = new();

    public IProvider? DataSource
    {
        get => _dataSource;
        set
        {
            if (_dataSource == value) return;
            _dataSource = value;
            OnPropertyChanged(nameof(DataSource));
            // This is a synchronous version so it doesn't need to be run in the Background
            // the Extent is already created on the creation of the Provider.
            Extent = DataSource?.GetExtent();
        }
    }

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        return _cache;
    }

    public void AbortFetch()
    {
        // not implemented for ImageLayer
    }

    public void RefreshData(FetchInfo fetchInfo, Action<Func<Task>> enqueueFetch)
    {
        if (!Enabled) return;
        // Fetching an image, that often covers the whole map, is expensive. Only do it on Discrete changes.
        if (fetchInfo.ChangeType == ChangeType.Continuous) return;

        var dataSource = DataSource;
        if (dataSource is null)
            return;

        Busy = true;
        Delayer.ExecuteDelayed(() => enqueueFetch!(() => FetchAsync(fetchInfo, ++_refreshCounter, dataSource, DateTime.Now.Ticks)), _delayBetweenCalls, 0);
    }

    private async Task FetchAsync(FetchInfo fetchInfo, int refreshCounter, IProvider dataSource, long timeRequested)
    {
        Busy = true;

        try
        {
            _cache = await dataSource.GetFeaturesAsync(fetchInfo);
            if (_refreshCounter == refreshCounter)
                Busy = false;
            OnDataChanged(new DataChangedEventArgs(Name));
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
            if (_refreshCounter == refreshCounter)
                Busy = false;
            OnDataChanged(new DataChangedEventArgs(ex, Name));
        }
    }

    public void ClearCache()
    {
        _cache = [];
    }

    public async Task<IDictionary<string, IEnumerable<IFeature>>> GetFeatureInfoAsync(Viewport viewport, ScreenPosition screenPosition)
    {
        if (DataSource is ILayerFeatureInfo featureInfo)
        {
            return await featureInfo.GetFeatureInfoAsync(viewport, screenPosition).ConfigureAwait(false);
        }

        return new Dictionary<string, IEnumerable<IFeature>>();
    }
}
