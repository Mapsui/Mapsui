// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Fetcher;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Layers;

/// <summary>
/// Create layer with name
/// </summary>
/// <param name="layerName">Name to use for layer</param>
public class Layer(string layerName) : BaseLayer(layerName), IFetchableSource, ILayerDataSource<IProvider>
{
    private IProvider? _dataSource;
    private readonly object _syncRoot = new();
    private IFeature[] _cache = [];
    private int _refreshCounter; // To determine if fetching is still Busy. Multiple refreshes can be in progress. To know if the last one was handled we use this counter.
    private readonly LatestMailbox<FetchInfo> _latestFetchInfo = new();

    public event EventHandler<FetchRequestedEventArgs>? FetchRequested;

    public List<Func<bool>> Animations { get; } = [];

    /// <summary>
    /// Create a new layer
    /// </summary>
    public Layer() : this("Layer") { }

    /// <summary>
    /// Data source for this layer
    /// </summary>
    public IProvider? DataSource
    {
        get => _dataSource;
        set
        {
            if (_dataSource == value) return;

            _dataSource = value;
            ClearCache();
            OnPropertyChanged(nameof(DataSource));
            OnPropertyChanged(nameof(Extent));
        }
    }

    /// <summary>
    /// Returns the extent of the layer
    /// </summary>
    /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
    public override MRect? Extent
    {
        get
        {
            lock (_syncRoot)
            {
                return DataSource?.GetExtent();
            }
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IFeature> GetFeatures(MRect extent, double resolution)
    {
        return _cache;
    }

    /// <inheritdoc />
    public void ClearCache()
    {
        _cache = [];
    }

    public override bool UpdateAnimations()
    {
        var areAnimationsRunning = false;
        foreach (var animation in Animations)
        {
            if (animation())
                areAnimationsRunning = true;
        }
        return areAnimationsRunning;
    }

    private async Task FetchAsync(FetchInfo fetchInfo)
    {
        if (fetchInfo.ChangeType == ChangeType.Continuous)
            throw new NotSupportedException("Continuous changes are not supported by ImageLayer.");

        var dataSource = DataSource;
        if (dataSource is null)
            return;

        await FetchAsync(fetchInfo, ++_refreshCounter);
    }

    public async Task FetchAsync(FetchInfo fetchInfo, int refreshCounter)
    {
        try
        {
            fetchInfo = fetchInfo.Grow(SymbolStyle.DefaultWidth);
            var features = DataSource != null ? await DataSource.GetFeaturesAsync(fetchInfo).ConfigureAwait(false) : [];
            _cache = features.ToArray();
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
