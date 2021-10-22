using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Fetcher
{
    class FeatureFetchDispatcher<T> : IFetchDispatcher where T : IFeature
    {
        private BoundingBox _extent;
        private double _resolution;
        private bool _busy;
        private readonly ConcurrentStack<T> _cache;
        private bool _modified;

        public FeatureFetchDispatcher(ConcurrentStack<T> cache)
        {
            _cache = cache;
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

        private void FetchCompleted(IEnumerable<T> features, Exception exception)
        {
            if (exception == null)
            {
                _cache.Clear();
                if (features.Any())
                {
                    _cache.PushRange(features.ToArray());
                }
            }

            Busy = _modified;

            DataChanged?.Invoke(this, new DataChangedEventArgs(exception, false, null));
        }

        public void SetViewport(BoundingBox extent, double resolution)
        {
            // Fetch a bigger extent to include partially visible symbols. 
            // todo: Take into account the maximum symbol size of the layer
            var grownExtent = extent.Grow(
                SymbolStyle.DefaultWidth * 2 * resolution,
                SymbolStyle.DefaultHeight * 2 * resolution);
            _extent = grownExtent.ToMRect().ToBoundingBox();
            _resolution = resolution;
            _modified = true;
            Busy = true;
        }

        public IProvider<T> DataSource { get; set; }

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
