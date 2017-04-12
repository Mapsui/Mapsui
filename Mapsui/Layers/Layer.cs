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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Utilities;

namespace Mapsui.Layers
{
    public class Layer : BaseLayer
    {
        private IProvider _dataSource;
        private object _syncRoot = new object();
        protected IEnumerable<IFeature> Cache;
        protected bool NeedsUpdate = true;
        protected BoundingBox NewExtent;
        protected double NewResolution;
        protected Timer FetchDelayTimer;
        
        public Layer() : this("Layer")
        {
        }

        public Layer(string layername) : base(layername)
        {
            Cache = new List<IFeature>();
            FetchDelayTimer = new Timer(FetchDelayTimerElapsed, FetchingPostponedInMilliseconds, int.MaxValue);
        }

        public IProvider DataSource
        {
            get { return _dataSource; }
            set
            {
                if (_dataSource == value) return;
                _dataSource = value;
                OnPropertyChanged(nameof(DataSource));
                OnPropertyChanged(nameof(Envelope));
                if (_dataSource != null) FetchDelayTimer.Start();
            }
        }

        public int FetchingPostponedInMilliseconds { get; set; } = 500;

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
                    var extent = DataSource?.GetExtents();
                    if (extent == null) return null;
                    if (ProjectionHelper.NeedsTransform(Transformation, CRS, DataSource.CRS))
                        return Transformation.Transform(DataSource.CRS, CRS, extent.Copy());
                    return extent;
                }
            }
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            return Cache;
        }

        public override void AbortFetch()
        {
        }

        public override void ViewChanged(bool majorChange, BoundingBox extent, double resolution)
        {
            if (!Enabled) return;
            if (DataSource == null) return;
            if (!majorChange) return;

            NewExtent = extent;
            NewResolution = resolution;

            if (Busy)
            {
                NeedsUpdate = true;
                return;
            }
            
            FetchDelayTimer.Restart();
        }

        private void FetchDelayTimerElapsed(object state)
        {
            if (NewExtent == null) return;
            FetchDelayTimer.Cancel();
            StartNewFetch(NewExtent, NewResolution);
        }

        protected void StartNewFetch(BoundingBox extent, double resolution)
        {
            Busy = true;
            NeedsUpdate = false;

            extent = Transform(extent);

            var fetcher = new FeatureFetcher(extent, resolution, DataSource, DataArrived);
            Task.Factory.StartNew(() => fetcher.FetchOnThread()); // Why Task.Factory iso Task.Run?
        }

        protected void DataArrived(IEnumerable<IFeature> features, object state = null)
        {
            if (features == null) throw new ArgumentException("argument features may not be null");

            try
            {
                Cache = Transform(features);
                OnDataChanged(new DataChangedEventArgs(null, false, null, Name));

                Busy = false;
                if (NeedsUpdate) StartNewFetch(NewExtent, NewResolution);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        }

        private BoundingBox Transform(BoundingBox extent)
        {
            if (ProjectionHelper.NeedsTransform(Transformation, CRS, DataSource.CRS))
                return Transformation.Transform(CRS, DataSource.CRS, extent.Copy());
            return extent;
        }

        private IEnumerable<IFeature> Transform(IEnumerable<IFeature> features)
        {
            if (!ProjectionHelper.NeedsTransform(Transformation, CRS, DataSource.CRS)) return features;

            var copiedFeatures = features.Copy().ToList();
            foreach (var feature in copiedFeatures)
            {
                if (feature.Geometry is Raster) continue;
                feature.Geometry = Transformation.Transform(DataSource.CRS, CRS, feature.Geometry.Copy());
            }
            return copiedFeatures;
        }

        public override void ClearCache()
        {
            Cache = null;
        }

        public override bool? IsCrsSupported(string crs)
        {
            if (Transformation == null) return null;
            if (DataSource == null) return null;
            return Transformation.IsProjectionSupported(DataSource.CRS, crs);
        }
    }
}