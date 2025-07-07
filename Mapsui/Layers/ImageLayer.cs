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

public class ImageLayer : BaseLayer, IFetchableSource, ILayerDataSource<IProvider>, IDisposable, ILayer, ILayerFeatureInfo
{
    private IEnumerable<IFeature> _cache = [];
    private IProvider? _dataSource;
    private int _refreshCounter; // To determine if fetching is still Busy. Multiple refreshes can be in progress. To know if the last one was handled we use this counter.
    private readonly LatestMailbox<FetchInfo> _latestFetchInfo = new();

    public event EventHandler<FetchRequestedEventArgs>? FetchRequested;

    public ImageLayer()
    {
        Style = new RasterStyle();
    }

    public bool NeedsFetch => !_latestFetchInfo.IsEmpty;

    public ImageLayer(string layerName) : this()
    {
        Name = layerName;
    }

    public IProvider? DataSource
    {
        get => _dataSource;
        set
        {
            if (_dataSource == value)
                return;
            _dataSource = value;
            ClearCache();
            OnPropertyChanged(nameof(DataSource));
            Extent = DataSource?.GetExtent();
            OnFetchRequested();
        }
    }

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        return _cache;
    }

    public FetchJob[] GetFetchJobs(int activeFetches, int availableFetchSlots)
    {
        if (!Enabled)
            return [];

        if (activeFetches > 0) // Allow only one fetch in progress for this layer type.
            return [];

        if (_latestFetchInfo.TryTake(out var fetchInfo))
            return [new FetchJob(Id, () => FetchAsync(fetchInfo))];
        return [];
    }

    private async Task FetchAsync(FetchInfo fetchInfo)
    {
        if (fetchInfo.ChangeType == ChangeType.Continuous)
            throw new NotSupportedException("Continuous changes are not supported by ImageLayer.");

        var dataSource = DataSource;
        if (dataSource is null)
            return;

        await FetchAsync(fetchInfo, ++_refreshCounter, dataSource, DateTime.Now.Ticks);
    }

    private async Task FetchAsync(FetchInfo fetchInfo, int refreshCounter, IProvider dataSource, long timeRequested)
    {
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

    public void ViewportChanged(FetchInfo fetchInfo)
    {
        Busy = true;
        _latestFetchInfo.Overwrite(fetchInfo);
    }

    protected virtual void OnFetchRequested()
    {
        FetchRequested?.Invoke(this, new FetchRequestedEventArgs(ChangeType.Discrete));
    }
}
