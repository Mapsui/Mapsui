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
    public class Layer : BaseLayer
    {
        private IProvider _dataSource;
        private readonly object _syncRoot = new object();
        private readonly MemoryProvider _cache = new MemoryProvider();
        private readonly FeatureFetchDispatcher _fetchDispatcher;
        private readonly FetchMachine _fetchMachine;
        private readonly Delayer _delayer = new Delayer();

        public Layer() : this("Layer") {}

        public Layer(string layername) : base(layername)
        {
            _fetchDispatcher = new FeatureFetchDispatcher(_cache, Transformer);
            _fetchDispatcher.DataChanged += FetchDispatcherOnDataChanged;
            _fetchDispatcher.PropertyChanged += FetchDispatcherOnPropertyChanged;

            _fetchMachine = new FetchMachine(_fetchDispatcher);
        }
        public int FetchingPostponedInMilliseconds { get; set; } = 500;

        public IProvider DataSource
        {
            get => _dataSource;
            set
            {
                if (_dataSource == value) return;
                _dataSource = value;
                
                Transformer.FromCRS = _dataSource?.CRS;

                _fetchDispatcher.DataSource = _dataSource;

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
        ///     Returns the extent of the layer
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

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            return _cache.Features;
        }

        public override void AbortFetch()
        {
            _fetchMachine.Stop();
        }

        public override void ViewChanged(bool majorChange, BoundingBox extent, double resolution)
        {
            if (!Enabled) return;
            if (DataSource == null) return;
            if (!majorChange) return;

            _delayer.ExecuteDelayed(() => DelayedFetch(extent.Copy(), resolution), FetchingPostponedInMilliseconds);
        }

        public override void ClearCache()
        {
            _cache.Clear();
        }

        public override bool? IsCrsSupported(string crs)
        {
            if (Transformation == null) return null;
            if (DataSource == null) return null;
            return Transformation.IsProjectionSupported(DataSource.CRS, crs);
        }
    }
}