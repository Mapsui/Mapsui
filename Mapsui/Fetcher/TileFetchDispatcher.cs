using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using BruTile;
using BruTile.Cache;
using ConcurrentCollections;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Fetcher
{
    class TileFetchDispatcher : IFetchDispatcher, INotifyPropertyChanged
    {
        private BoundingBox _extent;
        private double _resolution;
        private readonly object _lockRoot = new object();
        private bool _busy;
        private bool _viewportIsModified;
        private readonly ITileCache<Feature> _tileCache;
        private readonly IDataFetchStrategy _dataFetchStrategy;
        private readonly ConcurrentQueue<TileInfo> _tilesToFetch = new ConcurrentQueue<TileInfo>();
        private readonly ConcurrentHashSet<TileIndex> _tilesInProgress = new ConcurrentHashSet<TileIndex>();
        private readonly ITileSchema _tileSchema;
        private readonly FetchMachine _fetchMachine;
        private Func<TileInfo, Feature> _fetchTileAsFeature;

        public TileFetchDispatcher(
            ITileCache<Feature> tileCache, 
            ITileSchema tileSchema, 
            Func<TileInfo, Feature> fetchTileAsFeature, 
            IDataFetchStrategy dataFetchStrategy = null)
        {
            _tileCache = tileCache;
            _tileSchema = tileSchema;
            _fetchTileAsFeature = fetchTileAsFeature;
            _dataFetchStrategy = dataFetchStrategy ?? new MinimalDataFetchStrategy();
            _fetchMachine = new FetchMachine(this);
        }

        public event DataChangedEventHandler DataChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public int NumberTilesNeeded { get; private set; }

        public void SetViewport(BoundingBox extent, double resolution)
        {
            lock (_lockRoot)
            {
                _extent = extent;
                _resolution = resolution;
                Busy = true;
                _viewportIsModified = true;
            }
        }

        public bool TryTake(ref Action method)
        {
            lock (_lockRoot)
            {
                UpdateIfViewportIsModified();
                var success = _tilesToFetch.TryDequeue(out TileInfo tileInfo);

                if (success)
                {
                    _tilesInProgress.Add(tileInfo.Index);
                    method = () => FetchOnThread(tileInfo);
                    return true;
                }

                Busy = _tilesInProgress.Count > 0 || _tilesToFetch.Count > 0;
                // else the queue is empty, we are done.
                return false;
            }
        }

        private void FetchOnThread(TileInfo tileInfo)
        {
            try
            {
                var feature = _fetchTileAsFeature(tileInfo);
                FetchCompleted(tileInfo, feature, null);
            }
            catch (Exception exception)
            {
                FetchCompleted(tileInfo, null, exception);
            }
        }

        private void UpdateIfViewportIsModified()
        {
            if (_viewportIsModified)
            {
                UpdateTilesToFetchForViewportChange();
                _viewportIsModified = false;
            }
        }

        private void FetchCompleted(TileInfo tileInfo, Feature feature, Exception exception)
        {
            lock (_lockRoot)
            {
                if (exception == null)
                {
                    _tileCache.Add(tileInfo.Index, feature);
                }
                _tilesInProgress.TryRemove(tileInfo.Index);

                Busy = _tilesInProgress.Count > 0 || _tilesToFetch.Count > 0;

                DataChanged?.Invoke(this, new DataChangedEventArgs(exception, false, tileInfo));
            }
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

        public void StopFetching()
        {
            _fetchMachine?.Stop();
        }

        public void StartFetching()
        {
            _fetchMachine.Start();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateTilesToFetchForViewportChange()
        {
            var levelId = BruTile.Utilities.GetNearestLevel(_tileSchema.Resolutions, _resolution);
            var tilesToCoverViewport = _dataFetchStrategy.Get(_tileSchema, _extent.ToExtent(), levelId);
            NumberTilesNeeded = tilesToCoverViewport.Count;
            var tilesToFetch = tilesToCoverViewport.Where(t => _tileCache.Find(t.Index) == null && !_tilesInProgress.Contains(t.Index));
            _tilesToFetch.Clear();
            _tilesToFetch.AddRange(tilesToFetch);
            if (_tilesToFetch.Count > 0) Busy = true;
        }
    }
}
