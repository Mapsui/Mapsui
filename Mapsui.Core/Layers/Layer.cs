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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Mapsui.Fetcher;
using Mapsui.Providers;

namespace Mapsui.Layers
{
    public class Layer : BaseLayer, IAsyncDataFetcher
    {
        private IProvider<IFeature>? _dataSource;
        private readonly object _syncRoot = new();
        private readonly ConcurrentStack<IFeature> _cache = new();
        private readonly FeatureFetchDispatcher<IFeature> _fetchDispatcher;
        private readonly FetchMachine _fetchMachine;

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
            _fetchDispatcher = new FeatureFetchDispatcher<IFeature>(_cache);
            _fetchDispatcher.DataChanged += FetchDispatcherOnDataChanged;
            _fetchDispatcher.PropertyChanged += FetchDispatcherOnPropertyChanged;

            _fetchMachine = new FetchMachine(_fetchDispatcher);
        }

        /// <summary>
        /// Time to wait before fetching data
        /// </summary>
        // ReSharper disable once UnusedMember.Global // todo: Create a sample for this field
        public int FetchingPostponedInMilliseconds
        {
            get => Delayer.MillisecondsToWait;
            set => Delayer.MillisecondsToWait = value;
        }
        /// <summary>
        /// Data source for this layer
        /// </summary>
        public IProvider<IFeature>? DataSource
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

        private void FetchDispatcherOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Busy))
            {
                if (_fetchDispatcher != null) Busy = _fetchDispatcher.Busy;
            }
        }

        private void FetchDispatcherOnDataChanged(object sender, DataChangedEventArgs args)
        {
            OnDataChanged(args);
        }

        private void DelayedFetch(FetchInfo fetchInfo)
        {
            _fetchDispatcher.SetViewport(fetchInfo);
            _fetchMachine.Start();
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
            return _cache.ToList();
        }

        /// <inheritdoc />
        public void AbortFetch()
        {
            _fetchMachine.Stop();
        }

        /// <inheritdoc />
        public void ClearCache()
        {
            _cache.Clear();
        }

        /// <inheritdoc />
        public override void RefreshData(FetchInfo fetchInfo)
        {
            if (!Enabled) return;
            if (MinVisible > fetchInfo.Resolution) return;
            if (MaxVisible < fetchInfo.Resolution) return;
            if (DataSource == null) return;
            if (fetchInfo.ChangeType == ChangeType.Continuous) return;

            Delayer.ExecuteDelayed(() => DelayedFetch(fetchInfo));
        }
    }
}