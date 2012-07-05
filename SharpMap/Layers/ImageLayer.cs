// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
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
using System.Linq;
using SharpMap.Fetcher;
using SharpMap.Geometries;
using SharpMap.Providers;
using System.Threading;
using System.Collections.Generic;

namespace SharpMap.Layers
{
    public class ImageLayer : BaseLayer
    {
        protected bool isFetching;
        protected bool needsUpdate = true;
        protected double newResolution;
        protected BoundingBox newExtent;
        protected MemoryProvider cache;
        protected System.Timers.Timer startFetchTimer = new System.Timers.Timer(); 

        public IProvider DataSource { get; set; }

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
                    bool wasOpen = DataSource.IsOpen;
                    if (!wasOpen)
                        DataSource.Open();
                    BoundingBox box = DataSource.GetExtents();
                    if (!wasOpen) //Restore state
                        DataSource.Close();
                    if (Transformation != null && Transformation.MapSRID != -1 && SRID != -1)
                        return Transformation.Transfrom(SRID, Transformation.MapSRID, box);

                    return box;
                }
            }
        }

        public ImageLayer(string layername)
        {
            LayerName = layername;
            cache = new MemoryProvider();
            startFetchTimer.Interval = 500;
            startFetchTimer.Elapsed += StartFetchTimerElapsed;
        }

        void StartFetchTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StartNewFetch(newExtent, newResolution);
            startFetchTimer.Stop();
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            return cache.GetFeaturesInView(box, resolution);
        }

        public override void AbortFetch()
        {
        }

        public override void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            if (!Enabled) return;
            if (DataSource == null) return;
            if (!changeEnd) return;

            newExtent = extent;
            newResolution = resolution;

            if (isFetching)
            {
                needsUpdate = true;
                return;
            }
            startFetchTimer.Stop();
            startFetchTimer.Start();
        }

        protected void StartNewFetch(BoundingBox extent, double resolution)
        {
            isFetching = true;
            needsUpdate = false;

            if (Transformation != null && Transformation.MapSRID != -1 && SRID != -1)
                extent = Transformation.Transfrom(Transformation.MapSRID, SRID, extent);

            var fetcher = new FeatureFetcher(extent, resolution, DataSource, DataArrived);
            new Thread(fetcher.FetchOnThread).Start();
        }

        protected virtual void DataArrived(IEnumerable<IFeature> features)
        {
            //the data in the cache is stored in the map projection so it projected only once.
            if (features == null) throw new ArgumentException("argument features may not be null");

            features = features.ToList();
            if (Transformation != null && Transformation.MapSRID != -1 && SRID != -1 && SRID != Transformation.MapSRID)
            {
                foreach (var feature in features.Where(feature => !(feature.Geometry is Raster)))
                {
                    feature.Geometry = Transformation.Transform(SRID, Transformation.MapSRID, (Geometry)feature.Geometry);
                }
            }

            // get the newest feature in the old batch
            var newestOldFeature = cache.Features.OrderByDescending(f => ((IRaster) f.Geometry).TickFetched).FirstOrDefault();
            // creat list from feature so that the newest old feature can be added
            var list = features.ToList();
            // add it
            if (newestOldFeature != null) list.Add(newestOldFeature);
            // sort cache by time arrived
            cache = new MemoryProvider(list.OrderByDescending(f => ((IRaster)f.Geometry).TickFetched));
            // note: we should not sort by time arrived but by the time it was requested. 
            // Now it happens that some old requests that are take very long drawn on top
            
            isFetching = false;
            OnDataChanged();

            if (needsUpdate)
            {
                StartNewFetch(newExtent, newResolution);
            }
        }

        protected void OnDataChanged()
        {
            if (DataChanged != null)
            {
                DataChanged(this, new DataChangedEventArgs(null, false, null, LayerName));
            }
        }

        public override event DataChangedEventHandler DataChanged;

        public override void ClearCache()
        {
            cache.Clear();
        }
    }
}
