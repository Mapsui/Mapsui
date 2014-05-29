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

using System.Threading.Tasks;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Projection;
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
        protected IEnumerable<IFeature> Cache;
        protected Timer StartFetchTimer;

        public IProvider DataSource { get; set; }
        public int FetchingPostponedInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the CRS of this VectorLayer's data source
        /// </summary>
        private int SourceSRID
        {
            get
            {
                if (DataSource == null) throw (new Exception("DataSource is null on'" + LayerName + "'"));
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
                    var extent = DataSource.GetExtents();
                    if (Transformation != null && Transformation.MapSRID != -1 && SourceSRID != -1)
                        return Transformation.Transform(SourceSRID, Transformation.MapSRID, extent);
                    return extent;
                }
            }
        }

        public Layer() : this("Layer") { }

        public Layer(string layername) : base(layername)
        {
            Cache = new List<IFeature>(); 
            FetchingPostponedInMilliseconds = 500;
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

            extent = Transform(extent);

            var fetcher = new FeatureFetcher(extent, resolution, DataSource, DataArrived);
            Task.Factory.StartNew(() => fetcher.FetchOnThread(null));
        }

        protected void DataArrived(IEnumerable<IFeature> features, object state = null)
        {
            if (features == null) throw new ArgumentException("argument features may not be null");

            try
            {
                Cache = Transform(features);
                OnDataChanged(new DataChangedEventArgs(null, false, null, LayerName));

                IsFetching = false;
                if (NeedsUpdate) StartNewFetch(NewExtent, NewResolution);
            }
            catch (InvalidOperationException ex)
            {
            }
        }

        private BoundingBox Transform(BoundingBox extent)
        {
            if (!NeedsTransform(Transformation, SourceSRID)) return extent;
            extent = Transformation.Transform(Transformation.MapSRID, SourceSRID, CopyBoundingBox(extent));
            return extent;
        }

        private BoundingBox CopyBoundingBox(BoundingBox extent)
        {
            return new BoundingBox(extent.MinX, extent.MinY, extent.MaxX, extent.MaxY);
        }

        private IEnumerable<IFeature> Transform(IEnumerable<IFeature> features)
        {
            if (!NeedsTransform(Transformation, SourceSRID)) return features;
            
            var copiedFeatures = CopyFeatures(features).ToList();
            foreach (var feature in copiedFeatures.Where(feature => !(feature.Geometry is Raster)))
            {
                var geometry = Geometry.GeomFromWKB(feature.Geometry.AsBinary()); // copy geometry
                feature.Geometry = Transformation.Transform(SourceSRID, Transformation.MapSRID, geometry);
            }
            return copiedFeatures;
        }

        private static bool NeedsTransform(ITransformation transformation, int SRID)
        {
            return !(transformation == null || transformation.MapSRID == -1 || SRID == -1 || SRID == transformation.MapSRID);
        }

        private static IEnumerable<IFeature> CopyFeatures(IEnumerable<IFeature> features)
        {
            return features.Select(feature => new Feature(feature)).Cast<IFeature>().ToList();
        }

        public override void ClearCache()
        {
            Cache = null;
        }
    }
}
