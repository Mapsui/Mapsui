// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Layers;

public class ImageLayer : BaseLayer, IAsyncDataFetcher, ILayerDataSource<IProvider>, IDisposable, ILayer, ILayerFeatureInfo
{
    private class FeatureSets
    {
        public long TimeRequested { get; set; }
        public IEnumerable<RasterFeature> Features { get; set; } = [];
    }

    private bool _isFetching;
    private bool _needsUpdate = true;
    private FetchInfo? _fetchInfo;
    private List<FeatureSets> _sets = [];
    private readonly Timer? _startFetchTimer;
    private IProvider? _dataSource;
    private readonly int _numberOfFeaturesReturned;

    public ImageLayer()
    {
        Style = new RasterStyle();
        _startFetchTimer = new Timer(StartFetchTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        _numberOfFeaturesReturned = 1;
    }

    public ImageLayer(string layerName) : this()
    {
        Name = layerName;
    }

    /// <summary>
    /// Delay before fetching a new wms image from the server
    /// after the view has changed. Specified in milliseconds.
    /// </summary>
    public int FetchDelay { get; set; } = 1000;

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

    private void StartFetchTimerElapsed(object? state)
    {
        var fetchInfo = _fetchInfo;

        if (fetchInfo?.Section.Extent == null) return;
        if (double.IsNaN(fetchInfo.Section.Resolution)) return;
        StartNewFetch(fetchInfo);
    }

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        var result = new List<IFeature>();
        foreach (var featureSet in _sets.OrderBy(c => c.TimeRequested))
        {
            result.AddRange(GetFeaturesInView(box, featureSet.Features));
        }
        return result;
    }

    private static IEnumerable<IFeature> GetFeaturesInView(MRect box, IEnumerable<RasterFeature> features)
    {
        foreach (var feature in features)
        {
            if (feature.Raster == null)
                continue;

            if (box.Intersects(feature.Extent))
            {
                yield return feature;
            }
        }
    }

    public void AbortFetch()
    {
        // not implemented for ImageLayer
    }

    public void RefreshData(FetchInfo fetchInfo)
    {
        if (!Enabled) return;
        // Fetching an image, that often covers the whole map, is expensive. Only do it on Discrete changes.
        if (fetchInfo.ChangeType == ChangeType.Continuous) return;

        _fetchInfo = fetchInfo;
        Logger.Log(LogLevel.Debug, @$"Refresh Data: Resolution: {fetchInfo.Resolution} Change Type: {fetchInfo.ChangeType} Extent: {fetchInfo.Extent} ");

        Busy = true;
        if (_isFetching)
        {
            _needsUpdate = true;
            return;
        }

        _startFetchTimer?.Change(FetchDelay, Timeout.Infinite);
    }

    private void StartNewFetch(FetchInfo fetchInfo)
    {
        if (_dataSource == null) return;

        _isFetching = true;
        Busy = true;
        _needsUpdate = false;

        var fetcher = new FeatureFetcher(new FetchInfo(fetchInfo), _dataSource, DataArrived, DateTime.Now.Ticks);

        Catch.TaskRun(async () =>
        {
            try
            {
                Logger.Log(LogLevel.Debug, $"Start image fetch at {DateTime.Now.TimeOfDay}");
                await fetcher.FetchOnThreadAsync();
                Logger.Log(LogLevel.Debug, $"Finished image fetch at {DateTime.Now.TimeOfDay}");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
                OnDataChanged(new DataChangedEventArgs(ex, Name));
            }
        });
    }

    private void DataArrived(IEnumerable<IFeature>? arrivingFeatures, object? state)
    {
        ArgumentNullException.ThrowIfNull(arrivingFeatures);

        // The data in the cache is stored in the map projection so it projected only once.
        var features = arrivingFeatures.Cast<RasterFeature>().ToArray();

        // We can get 0 features if some error was occurred up call stack
        // We should not add new FeatureSets if we have not any feature

        _isFetching = false;

        if (features.Length != 0)
        {
            _sets.Add(new FeatureSets { TimeRequested = state == null ? 0 : (long)state, Features = features });

            // Keep only two most recent sets. The older ones will be removed
            _sets = _sets.OrderByDescending(c => c.TimeRequested).Take(_numberOfFeaturesReturned).ToList();

            OnDataChanged(new DataChangedEventArgs(null, Name));
        }

        if (_needsUpdate)
        {
            if (_fetchInfo != null) StartNewFetch(_fetchInfo);
        }
        else
        {
            Busy = false;
        }
    }

    public void ClearCache()
    {
        foreach (var cache in _sets)
        {
            cache.Features = [];
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _startFetchTimer?.Dispose();
        }

        base.Dispose(disposing);
    }

    public async Task<IDictionary<string, IEnumerable<IFeature>>> GetFeatureInfoAsync(Viewport viewport, double screenX, double screenY)
    {
        if (DataSource is ILayerFeatureInfo featureInfo)
        {
            return await featureInfo.GetFeatureInfoAsync(viewport, screenX, screenY).ConfigureAwait(false);
        }

        return new Dictionary<string, IEnumerable<IFeature>>();
    }
}
