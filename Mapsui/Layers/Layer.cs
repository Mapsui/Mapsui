// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Fetcher;
using Mapsui.Providers;

namespace Mapsui.Layers;

public class Layer : BaseLayer, IAsyncDataFetcher, ILayerDataSource<IProvider>
{
    private IProvider? _dataSource;
    private readonly object _syncRoot = new();
    private IFeature[] _cache = [];
    private readonly FeatureFetchDispatcher _fetchDispatcher;
    private readonly FeatureFetchMachine _fetchMachine;
    private int busyCounter;

    public List<Func<bool>> Animations { get; } = [];
    public Delayer Delayer { get; } = new();

    /// <summary>
    /// Create a new layer
    /// </summary>
    public Layer() : this("Layer") { }

    /// <summary>
    /// Create layer with name
    /// </summary>
    /// <param name="layerName">Name to use for layer</param>
    public Layer(string layerName) : base(layerName)
    {
        _fetchDispatcher = new FeatureFetchDispatcher();
        _fetchMachine = new FeatureFetchMachine();
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

            if (_dataSource != null)
            {
                _fetchDispatcher.DataSource = _dataSource;
            }

            OnPropertyChanged(nameof(DataSource));
            OnPropertyChanged(nameof(Extent));
        }
    }

    private void DelayedFetch(FetchInfo fetchInfo)
    {
        _fetchDispatcher.SetViewport(fetchInfo);
        _fetchMachine.Start(FetchAsync);
    }

    private async Task FetchAsync()
    {
        await _fetchDispatcher.FetchAsync((r) =>
        {
            BusyMinusOne();
            r.Handle(
                (f) =>
                {
                    _cache = f;
                    OnDataChanged(new DataChangedEventArgs());
                },
                (e) => OnDataChanged(new DataChangedEventArgs(e)));
        });
    }

    private void BusyPlusOne()
    {
        lock (_syncRoot)
        {
            busyCounter++;
            Busy = true;
        }
    }

    private void BusyMinusOne()
    {
        lock (_syncRoot)
        {
            busyCounter--;
            if (busyCounter == 0)
                Busy = false;
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

        BusyPlusOne();
        Delayer.ExecuteDelayed(() => DelayedFetch(fetchInfo));
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
}
