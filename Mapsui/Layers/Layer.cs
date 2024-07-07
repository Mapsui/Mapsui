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
public class Layer(string layerName) : BaseLayer(layerName), IAsyncDataFetcher, ILayerDataSource<IProvider>
{
    private IProvider? _dataSource;
    private readonly object _syncRoot = new();
    private IFeature[] _cache = [];
    private readonly FetchMachine _fetchMachine = new();
    private int _refreshCounter; // To determine if fetching is still Busy. Multiple refreshes can be in progress. To know if the last one was handled we use this counter.

    public List<Func<bool>> Animations { get; } = [];
    public Delayer Delayer { get; } = new();

    /// <summary>
    /// Create a new layer
    /// </summary>
    public Layer() : this("Layer") { }

    /// <summary>
    /// Time to wait before fetching data
    /// </summary>
    public int FetchingPostponedInMilliseconds
    {
        get => Delayer.MillisecondsBetweenCalls;
        set => Delayer.MillisecondsBetweenCalls = value;
    }

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
    public void AbortFetch()
    {
        _fetchMachine.Stop();
    }

    /// <inheritdoc />
    public void ClearCache()
    {
        _cache = [];
    }

    /// <inheritdoc />
    public void RefreshData(FetchInfo fetchInfo)
    {
        if (!Enabled) return;
        if (MinVisible > fetchInfo.Resolution) return;
        if (MaxVisible < fetchInfo.Resolution) return;
        if (DataSource == null) return;
        if (fetchInfo.ChangeType == ChangeType.Continuous) return;

        Busy = true;
        Delayer.ExecuteDelayed(() => _fetchMachine.Start(() => FetchAsync(fetchInfo, ++_refreshCounter)));
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

    public async Task FetchAsync(FetchInfo fetchInfo, int refreshCounter)
    {
        fetchInfo = fetchInfo.Grow(SymbolStyle.DefaultWidth);

        try
        {
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
}
