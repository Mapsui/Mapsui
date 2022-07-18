// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Logging;
using Mapsui.Providers;

namespace Mapsui.Layers
{
    public class ImageLayer : BaseLayer, IAsyncDataFetcher, ILayerDataSource<IProvider>, IDisposable
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _startFetchTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        private class FeatureSets
        {
            public long TimeRequested { get; set; }
            public IEnumerable<RasterFeature> Features { get; set; } = new List<RasterFeature>();
        }

        private bool _isFetching;
        private bool _needsUpdate = true;
        private FetchInfo? _fetchInfo;
        private List<FeatureSets> _sets = new();
        private readonly Timer _startFetchTimer;
        private IProvider? _dataSource;
        private readonly int _numberOfFeaturesReturned;

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
                _dataSource = value;
                OnPropertyChanged(nameof(DataSource));
            }
        }

        public ImageLayer(string layerName)
        {
            Name = layerName;
            _startFetchTimer = new Timer(StartFetchTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _numberOfFeaturesReturned = 1;
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataSource))
            {
                Catch.TaskRun(() => {
                    // Run in background because it could take time because
                    // this could involve database access or a web request
                    Extent = DataSource?.GetExtent();
                });
            }
        }

        private void StartFetchTimerElapsed(object? state)
        {
            if (_fetchInfo?.Extent == null) return;
            if (double.IsNaN(_fetchInfo.Resolution)) return;
            StartNewFetch(_fetchInfo);
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

            if (_isFetching)
            {
                _needsUpdate = true;
                return;
            }

            _startFetchTimer.Change(FetchDelay, Timeout.Infinite);
        }

        private void StartNewFetch(FetchInfo fetchInfo)
        {
            if (_dataSource == null) return;

            _isFetching = true;
            _needsUpdate = false;

            var fetcher = new FeatureFetcher(new FetchInfo(fetchInfo), _dataSource, DataArrived, DateTime.Now.Ticks);

            Catch.TaskRun(async () => {
                try
                {
                    Logger.Log(LogLevel.Debug, $"Start image fetch at {DateTime.Now.TimeOfDay}");
                    await fetcher.FetchOnThreadAsync();
                    Logger.Log(LogLevel.Debug, $"Finished image fetch at {DateTime.Now.TimeOfDay}");
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex.Message, ex);
                    OnDataChanged(new DataChangedEventArgs(ex, false, null));
                }
            });
        }

        private void DataArrived(IEnumerable<IFeature>? arrivingFeatures, object? state)
        {
            //the data in the cache is stored in the map projection so it projected only once.
            var features = arrivingFeatures?.Cast<RasterFeature>().ToList() ?? throw new ArgumentException("argument features may not be null");

            // We can get 0 features if some error was occurred up call stack
            // We should not add new FeatureSets if we have not any feature

            _isFetching = false;

            if (features.Any())
            {
                features = features.ToList();

                _sets.Add(new FeatureSets { TimeRequested = state == null ? 0 : (long)state, Features = features });

                //Keep only two most recent sets. The older ones will be removed
                _sets = _sets.OrderByDescending(c => c.TimeRequested).Take(_numberOfFeaturesReturned).ToList();

                OnDataChanged(new DataChangedEventArgs(null, false, null, Name));
            }

            if (_needsUpdate)
            {
                if (_fetchInfo != null) StartNewFetch(_fetchInfo);
            }
        }

        public void ClearCache()
        {
            foreach (var cache in _sets)
            {
                cache.Features = new List<RasterFeature>();
            }
        }
    }
}
