using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Fetcher
{
    class FeatureFetchDispatcher : IFetchDispatcher
    {
        private BoundingBox _extent;
        private double _resolution;
        private readonly object _lockRoot = new object();
        private bool _busy;
        private readonly MemoryProvider _cache;
        private readonly Transformer _transformer;
        private bool _modified;

        // todo: Check whether busy and modified state are set correctly in all stages

        public FeatureFetchDispatcher(MemoryProvider cache, Transformer transformer)
        {
            _cache = cache;
            _transformer = transformer;
        }

        public bool TryTake(ref Action method)
        {
            if (!_modified) return false;
            if (DataSource == null) return false; 

            method = () => FetchOnThread(_extent.Copy(), _resolution);
            _modified = false;
            return true;
        }

        public void FetchOnThread(BoundingBox extent, double resolution)
        {
            try
            {
                var features = DataSource.GetFeaturesInView(extent, resolution).ToList();
                FetchCompleted(features, null);
            }
            catch (Exception exception)
            {
                FetchCompleted(null, exception);
            }
        }

        private void FetchCompleted(IEnumerable<IFeature> features, Exception exception)
        {
            lock (_lockRoot)
            {
                if (exception == null)
                {
                    _cache.ReplaceFeatures(_transformer.Transform(features));
                }
                
                Busy = _modified;

                DataChanged?.Invoke(this, new DataChangedEventArgs(exception, false, null));
            }
        }

        public void SetViewport(BoundingBox extent, double resolution)
        {
            lock (_lockRoot)
            {
                // Fetch a bigger extent to include partially visible symbols. 
                // todo: Take into account the maximum symbol size of the layer
                var grownExtent = extent.Grow(
                    SymbolStyle.DefaultWidth * 2 * resolution,
                    SymbolStyle.DefaultHeight * 2 * resolution);
                var transformedExtent = _transformer.TransformBack(grownExtent);
                _extent = transformedExtent;
                _resolution = resolution;
                _modified = true;
                Busy = true;
            }
        }

        public IProvider DataSource { get; set; }

        public bool Busy
        {
            get => _busy;
            private set
            {
                if (_busy == value) return; // prevent notify              
                _busy = value;
                OnPropertyChanged(nameof(Busy));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event DataChangedEventHandler DataChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
