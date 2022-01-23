using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Fetcher
{
    internal class FeatureFetchDispatcher<T> : IFetchDispatcher where T : IFeature
    {
        private FetchInfo? _fetchInfo;
        private bool _busy;
        private readonly ConcurrentStack<T> _cache;
        private bool _modified;

        public FeatureFetchDispatcher(ConcurrentStack<T> cache)
        {
            _cache = cache;
        }

        public bool TryTake([NotNullWhen(true)] out Action? method)
        {
            method = null;
            if (!_modified) return false;
            if (_fetchInfo == null) return false;

            method = () => FetchOnThread(new FetchInfo(_fetchInfo));
            _modified = false;
            return true;
        }

        public void FetchOnThread(FetchInfo fetchInfo)
        {
            try
            {
                var features = DataSource?.GetFeatures(fetchInfo).ToList();
                FetchCompleted(features, null);
            }
            catch (Exception exception)
            {
                FetchCompleted(null, exception);
            }
        }

        private void FetchCompleted(IEnumerable<T>? features, Exception? exception)
        {
            if (exception == null)
            {
                _cache.Clear();
                if (features.Any())
                    _cache.PushRange(features.ToArray());
            }

            Busy = _modified;

            DataChanged?.Invoke(this, new DataChangedEventArgs(exception, false, null));
        }

        public void SetViewport(FetchInfo fetchInfo)
        {
            // Fetch a bigger extent to include partially visible symbols. 
            // todo: Take into account the maximum symbol size of the layer

            var biggerBox = fetchInfo.Extent.Grow(
                SymbolStyle.DefaultWidth * 2 * fetchInfo.Resolution,
                SymbolStyle.DefaultHeight * 2 * fetchInfo.Resolution);
            _fetchInfo = new FetchInfo(biggerBox, fetchInfo.Resolution, fetchInfo.CRS, fetchInfo.ChangeType);


            _modified = true;
            Busy = true;
        }

        public IProvider<T>? DataSource { get; set; }

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

        public event DataChangedEventHandler? DataChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
