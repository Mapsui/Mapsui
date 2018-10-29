// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Logging;
using Mapsui.Utilities;

namespace Mapsui.Layers
{
    public class ImageLayer : BaseLayer, IAsyncDataFetcher
    {
        private class FeatureSets
        {
            public long TimeRequested { get; set; }
            public IEnumerable<IFeature> Features { get; set; }
        }

        private bool _isFetching;
        private bool _needsUpdate = true;
        private double _newResolution;
        private BoundingBox _newExtent;
        private List<FeatureSets> _sets = new List<FeatureSets>();
        private readonly Timer _startFetchTimer;
        private IProvider _dataSource;
        private readonly int _numberOfFeaturesReturned;

        /// <summary>
        /// Delay before fetching a new wms image from the server
        /// after the view has changed. Specified in milliseconds.
        /// </summary>
        public int FetchDelay { get; set; } = 1000;

        public IProvider DataSource
        {
            get => _dataSource;
            set
            {
                _dataSource = value;
                OnPropertyChanged(nameof(DataSource));
            }
        }

        public ImageLayer(string layername)
        {
            Name = layername;
            _startFetchTimer = new Timer(StartFetchTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _numberOfFeaturesReturned = 1;
            PropertyChanged += OnPropertyChanged;
        }

        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CRS) ||
                e.PropertyName == nameof(DataSource) ||
                e.PropertyName == nameof(Transformation))
            {
                Task.Run(() => // Run in background because it could take time.
                {
                    var sourceExtent = DataSource.GetExtents(); // This method could involve database access or a web request
                    Envelope = ProjectionHelper.GetTransformedBoundingBox(
                        Transformation, sourceExtent, DataSource.CRS, CRS);
                });
            }
        }

        void StartFetchTimerElapsed(object state)
        {
            if (_newExtent == null) return;
            if (double.IsNaN(_newResolution)) return;
            StartNewFetch(_newExtent, _newResolution);
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var result = new List<IFeature>();
            foreach (var featureSet in _sets.OrderBy(c => c.TimeRequested))
            {
                result.AddRange(GetFeaturesInView(box, featureSet.Features));
            }
            return result;
        }

        private static IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, IEnumerable<IFeature> features)
        {
            foreach (var feature in features)
            {
                if (feature.Geometry == null)
                    continue;

                if (box.Intersects(feature.Geometry.BoundingBox))
                {
                    yield return feature;
                }
            }
        }

        public void AbortFetch()
        {
            // not implemented for ImageLayer
        }

        public override void RefreshData(BoundingBox extent, double resolution, bool majorChange)
        {
            if (!Enabled) return;
            if (DataSource == null) return;
            if (!majorChange) return;

            _newExtent = extent;
            _newResolution = resolution;

            if (_isFetching)
            {
                _needsUpdate = true;
                return;
            }

            _startFetchTimer.Change(FetchDelay, Timeout.Infinite);
        }

        private void StartNewFetch(BoundingBox extent, double resolution)
        {
            _isFetching = true;
            _needsUpdate = false;

            var newExtent = new BoundingBox(extent);

            if (Transformation != null && !string.IsNullOrWhiteSpace(CRS)) DataSource.CRS = CRS;

            if (ProjectionHelper.NeedsTransform(Transformation, CRS, DataSource.CRS))
                if (Transformation != null && Transformation.IsProjectionSupported(CRS, DataSource.CRS) == true)
                    newExtent = Transformation.Transform(CRS, DataSource.CRS, extent);

            var fetcher = new FeatureFetcher(newExtent, resolution, DataSource, DataArrived, DateTime.Now.Ticks);

            Task.Run(() =>
            {
                Logger.Log(LogLevel.Debug, $"Start image fetch at {DateTime.Now.TimeOfDay}");
                fetcher.FetchOnThread();
                Logger.Log(LogLevel.Debug, $"Finished image fetch at {DateTime.Now.TimeOfDay}");
            });
        }

        private void DataArrived(IEnumerable<IFeature> features, object state)
        {
            //the data in the cache is stored in the map projection so it projected only once.
            features = features?.ToList() ?? throw new ArgumentException("argument features may not be null");

            // We can get 0 features if some error was occured up call stack
            // We should not add new FeatureSets if we have not any feature

            _isFetching = false;

            if (features.Any())
            {
                features = features.ToList();
                if (ProjectionHelper.NeedsTransform(Transformation, CRS, DataSource.CRS))
                {
                    foreach (var feature in features.Where(feature => !(feature.Geometry is Raster)))
                    {
                        feature.Geometry = Transformation.Transform(DataSource.CRS, CRS, feature.Geometry);
                    }
                }

                _sets.Add(new FeatureSets { TimeRequested = (long)state, Features = features });

                //Keep only two most recent sets. The older ones will be removed
                _sets = _sets.OrderByDescending(c => c.TimeRequested).Take(_numberOfFeaturesReturned).ToList();

                OnDataChanged(new DataChangedEventArgs(null, false, null, Name));
            }

            if (_needsUpdate)
            {
                StartNewFetch(_newExtent, _newResolution);
            }
        }

        public void ClearCache()
        {
            foreach (var cache in _sets)
            {
                cache.Features = new Features();
            }
        }

        public override bool? IsCrsSupported(string crs)
        {
            var projectingProvider = (DataSource as IProjectingProvider);
            if (projectingProvider == null) return (crs == CRS);
            return projectingProvider.IsCrsSupported(crs);
        }
    }
}
