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
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Utilities;

namespace Mapsui.Layers
{
    public class ImageLayer : BaseLayer
    {
        protected class FeatureSets
        {
            public long TimeRequested { get; set; }
            public IEnumerable<IFeature> Features { get; set; }
        }

        protected bool IsFetching;
        protected bool NeedsUpdate = true;
        protected double NewResolution;
        protected BoundingBox NewExtent;
        protected List<FeatureSets> Sets = new List<FeatureSets>();
        protected Timer StartFetchTimer;
        private IProvider _dataSource;
        public int NumberOfFeaturesReturned { get; set; }


        public IProvider DataSource
        {
            get { return _dataSource; }
            set
            {
                if (_dataSource == value) return;
                _dataSource = value;
                OnPropertyChanged("DataSource");
                OnPropertyChanged("Envelope");

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
                    return ProjectionHelper.GetTransformedBoundingBox(Transformation, DataSource.GetExtents(), DataSource.CRS, CRS);                   
                }
            }
        }

        public ImageLayer(string layername)
        {
            Name = layername;
            StartFetchTimer = new Timer(StartFetchTimerElapsed, null, 500, int.MaxValue);
            NumberOfFeaturesReturned = 1;
        }

        void StartFetchTimerElapsed(object state)
        {
            if (NewExtent == null) return;
            if (double.IsNaN(NewResolution)) return;
            StartNewFetch(NewExtent, NewResolution);
            StartFetchTimer.Dispose();
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var result = new List<IFeature>();
            foreach (var featureSet in Sets.OrderBy(c => c.TimeRequested))
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

                if (box.Intersects(feature.Geometry.GetBoundingBox()))
                {
                    yield return feature;
                }
            }
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
            StartFetchTimer.Dispose();
            StartFetchTimer = new Timer(StartFetchTimerElapsed, null, 500, int.MaxValue);
        }

        protected void StartNewFetch(BoundingBox extent, double resolution)
        {
            IsFetching = true;
            NeedsUpdate = false;

            var newExtent = new BoundingBox(extent);
            
            if (Transformation != null && !string.IsNullOrWhiteSpace(CRS)) DataSource.CRS = CRS;

            if (ProjectionHelper.NeedsTransform(Transformation, CRS, DataSource.CRS))
                if (Transformation != null && Transformation.IsProjectionSupported(CRS, DataSource.CRS) == true)
                    newExtent = Transformation.Transform(CRS, DataSource.CRS, extent);
                

            var fetcher = new FeatureFetcher(newExtent, resolution, DataSource, DataArrived, DateTime.Now.Ticks);
            Task.Run(() => fetcher.FetchOnThread());
        }

        protected virtual void DataArrived(IEnumerable<IFeature> features, object state)
        {
            //the data in the cache is stored in the map projection so it projected only once.
            if (features == null) throw new ArgumentException("argument features may not be null");

            features = features.ToList();
            if (ProjectionHelper.NeedsTransform(Transformation, CRS, DataSource.CRS))
            {
                foreach (var feature in features.Where(feature => !(feature.Geometry is Raster)))
                {
                    feature.Geometry = Transformation.Transform(DataSource.CRS, CRS, feature.Geometry);
                }
            }

            Sets.Add(new FeatureSets { TimeRequested = (long)state, Features = features });

            //Keep only two most recent sets. The older ones will be removed
            Sets = Sets.OrderByDescending(c => c.TimeRequested).Take(NumberOfFeaturesReturned).ToList();

            IsFetching = false;
            OnDataChanged(new DataChangedEventArgs(null, false, null, Name));

            if (NeedsUpdate)
            {
                StartNewFetch(NewExtent, NewResolution);
            }
        }

        public override void ClearCache()
        {
            foreach (var cache in Sets)
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
