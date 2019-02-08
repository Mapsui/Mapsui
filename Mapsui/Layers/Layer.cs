// TODO: There are parts talking about SharpMap

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

using System.Collections.Generic;
using System.ComponentModel;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Utilities;

// todo: Use Transformer only to translate between provider and cache. Layer only interacts with cache.
// todo: Put the datasource envelop in the cache (it should not just be the envelope of the cached data, but all data in datasource). 

namespace Mapsui.Layers
{
    public class Layer : BaseLayer, IAsyncDataFetcher
    {
        private IProvider _dataSource;
        private readonly object _syncRoot = new object();
        private readonly MemoryProvider _cache = new MemoryProvider();
        private readonly FeatureFetchDispatcher _fetchDispatcher;
        private readonly FetchMachine _fetchMachine;
        private readonly Delayer _delayer = new Delayer();

        /// <summary>
        /// Create a new layer
        /// </summary>
        public Layer() : this("Layer") {}

        /// <summary>
        /// Create layer with name
        /// </summary>
        /// <param name="layername">Name to use for layer</param>
        public Layer(string layername) : base(layername)
        {
            _fetchDispatcher = new FeatureFetchDispatcher(_cache, Transformer);
            _fetchDispatcher.DataChanged += FetchDispatcherOnDataChanged;
            _fetchDispatcher.PropertyChanged += FetchDispatcherOnPropertyChanged;

            _fetchMachine = new FetchMachine(_fetchDispatcher);
        }

        /// <summary>
        /// Time to wait before fetching data
        /// </summary>
        public int FetchingPostponedInMilliseconds { get; set; } = 500;

        /// <summary>
        /// Data source for this layer
        /// </summary>
        public IProvider DataSource
        {
            get => _dataSource;
            set
            {
                if (_dataSource == value) return;

                _dataSource = value;
                ClearCache();
                
                if (_dataSource != null)
                {
                    Transformer.FromCRS = _dataSource?.CRS;
                    _fetchDispatcher.DataSource = _dataSource;
                }

                OnPropertyChanged(nameof(DataSource));
                OnPropertyChanged(nameof(Envelope));
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

        private void DelayedFetch(BoundingBox extent, double resolution)
        {
            _fetchDispatcher.SetViewport(extent, resolution);
            _fetchMachine.Start();
        }
        
        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public override BoundingBox Envelope
        {
            get
            {
                lock (_syncRoot)
                {
                    return ProjectionHelper.Transform(DataSource?.GetExtents(), Transformation, DataSource?.CRS, CRS);
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            return _cache.Features;
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
        public override void RefreshData(BoundingBox extent, double resolution, bool majorChange)
        {
            if (!Enabled) return;
            if (DataSource == null) return;
            if (!majorChange) return;

            _delayer.ExecuteDelayed(() => DelayedFetch(extent.Copy(), resolution), FetchingPostponedInMilliseconds);
        }

        /// <inheritdoc />
        public override bool? IsCrsSupported(string crs)
        {
            if (Transformation == null) return null;
            if (DataSource == null) return null;
            return Transformation.IsProjectionSupported(DataSource.CRS, crs);
        }
    }
}