// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mapsui.Layers
{
    public class Layer : BaseLayer
    {
        protected bool IsFetching;
        protected bool NeedsUpdate = true;
        protected double NewResolution;
        protected BoundingBox NewExtent;
        protected MemoryProvider Cache;
        protected Timer StartFetchTimer; 
        
        public IProvider DataSource { get; set; }
        public int FetchingPostponedInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the SRID of this VectorLayer's data source
        /// </summary>
        public new int SRID
        {
            get
            {
                if (DataSource == null)
                    throw (new Exception("DataSource property not set on layer '" + LayerName + "'"));
                return DataSource.SRID;
            }
        }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public override BoundingBox Envelope
        {
            get
            {
                if (DataSource == null) return null;

                lock (DataSource)
                {
                    var box = DataSource.GetExtents();
                    if (Transformation != null && Transformation.MapSRID != -1 && SRID != -1)
                        return Transformation.Transfrom(SRID, Transformation.MapSRID, box);
                    return box;
                }
            }
        }

        public Layer() : this("Layer") { }

        public Layer(string layername) : base(layername)
        {
            Cache = new MemoryProvider();
            FetchingPostponedInMilliseconds = 500;
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            return Cache.GetFeaturesInView(box, resolution);
        }

        public override void AbortFetch()
        {
        }

        public override void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            if (!Enabled) return;
            if (DataSource == null) return;
            if (!changeEnd) return;

            NewExtent = extent;
            NewResolution = resolution;

            if (IsFetching)
            {
                NeedsUpdate = true;
                return;
            }
            if (StartFetchTimer != null) StartFetchTimer.Dispose();
            StartFetchTimer = new Timer(StartFetchTimerElapsed, null, FetchingPostponedInMilliseconds, int.MaxValue);
        }
        
        void StartFetchTimerElapsed(object state)
        {
            if (NewExtent == null) return;
            StartNewFetch(NewExtent, NewResolution);
            StartFetchTimer.Dispose();
        }

        protected void StartNewFetch(BoundingBox extent, double resolution)
        {
            IsFetching = true;
            NeedsUpdate = false;

            if (Transformation != null && Transformation.MapSRID != -1 && SRID != -1)
                extent = Transformation.Transfrom(Transformation.MapSRID, SRID, extent);

            var fetcher = new FeatureFetcher(extent, resolution, DataSource, DataArrived);
            ThreadPool.QueueUserWorkItem(fetcher.FetchOnThread);
        }

        protected void DataArrived(IEnumerable<IFeature> features, object state = null)
        {
            //the data in the cache is stored in the map projection so it projected only once.
            if (features == null) throw new ArgumentException("argument features may not be null");

            /* 
             * todo: 
             * Temporarily try catch added, fix for when features.ToList() crashes on InvalidOperationException 
             * happens sometimes with a really slow internet connection
             */
            try
            {
                features = features.ToList();
                if (Transformation != null && Transformation.MapSRID != -1 && SRID != -1 && SRID != Transformation.MapSRID)
                {
                    foreach (var feature in features.Where(feature => !(feature.Geometry is Raster)))
                    {
                        feature.Geometry = Transformation.Transform(SRID, Transformation.MapSRID, (Geometry)feature.Geometry);
                    }
                }

                Cache = new MemoryProvider(features);

                IsFetching = false;
                OnDataChanged(new DataChangedEventArgs(null, false, null, LayerName));

                if (NeedsUpdate)
                {
                    StartNewFetch(NewExtent, NewResolution);
                }
            }
            catch (InvalidOperationException) { }
        }

        public override void ClearCache()
        {
            Cache.Clear();
        }
    }
}
