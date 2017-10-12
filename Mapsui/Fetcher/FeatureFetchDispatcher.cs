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
        private IProvider _dataSource;
        private readonly Transformer _transformer;
        private bool _modified;

        // todo: Check wether busy and modified state are set correctly in all stages

        public FeatureFetchDispatcher(MemoryProvider cache, IProvider dataSource, Transformer transformer)
        {
            _cache = cache;
            _dataSource = dataSource;
            _transformer = transformer;
        }

        public bool TryTake(ref Action method)
        {
            if (!_modified) return false;
            if (_dataSource == null) return false; 

            method = () => FetchOnThread(_extent.Copy(), _resolution);
            _modified = false;
            return true;
        }

        public void FetchOnThread(BoundingBox extent, double resolution)
        {
            try
            {
                var features = _dataSource.GetFeaturesInView(extent, resolution).ToList();
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
                    _cache.Features.Clear();
                    var transformedFeatures = _transformer.Transform(features);
                    foreach (var feature in transformedFeatures)
                    {
                        _cache.Features.Add(feature);
                    }
                    
                }
                
                Busy = _modified;

                DataChanged?.Invoke(this, new DataChangedEventArgs(exception, false, null));
            }
        }

        public void SetViewport(BoundingBox extent, double resolution)
        {
            lock (_lockRoot)
            {
                var transformedExtent = _transformer.TransformBack(extent);
                var grownExtent = transformedExtent.Grow(
                    SymbolStyle.DefaultWidth * 2 * resolution, 
                    SymbolStyle.DefaultHeight * 2 * resolution);
                _extent = grownExtent;
                _resolution = resolution;
                _modified = true;
                Busy = true;
            }
        }

        public IProvider DataSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

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
