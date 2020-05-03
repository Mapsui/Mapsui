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
        private bool _modified;
        private readonly ITileCache<Feature> _tileCache;
        private readonly IFetchStrategy _fetchStrategy;
        private ConcurrentQueue<TileInfo> _tilesMissing = new ConcurrentQueue<TileInfo>();
        private readonly ConcurrentHashSet<TileIndex> _tilesInProgress = new ConcurrentHashSet<TileIndex>();
        private ITileSchema _tileSchema;
        private readonly FetchMachine _fetchMachine;
        private Func<TileInfo, Feature> _tileFetcher;

        public TileFetchDispatcher(ITileCache<Feature> tileCache, ITileSchema tileSchema, 
            Func<TileInfo, Feature> tileFetcher, IFetchStrategy fetchStrategy = null)
        {
            _tileCache = tileCache;
            _tileSchema = tileSchema;
            _tileFetcher = tileFetcher;
            _fetchStrategy = fetchStrategy ?? new MinimalFetchStrategy();
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
                _modified = true;
            }
        }

        public bool TryTake(ref Action method)
        {
            lock (_lockRoot)
            {
                UpdateIfModified();
                var success = _tilesMissing.TryDequeue(out TileInfo tileInfo);

                if (success)
                {
                    _tilesInProgress.Add(tileInfo.Index);
                    method = () => FetchOnThread(tileInfo);
                    return true;
                }

                Busy = _tilesInProgress.Count > 0 || _tilesMissing.Count > 0;
                // else the queue is empty, we are done.
                return false;
            }
        }

        private void FetchOnThread(TileInfo tileInfo)
        {
            try
            {
                var feature = _tileFetcher(tileInfo);
                FetchCompleted(tileInfo, feature, null);
            }
            catch (Exception exception)
            {
                FetchCompleted(tileInfo, null, exception);
            }
        }

        private void UpdateIfModified()
        {
            if (_modified)
            {
                UpdateMissingTiles();
                _modified = false;
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

                Busy = _tilesInProgress.Count > 0 || _tilesMissing.Count > 0;

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

        private void UpdateMissingTiles()
        {
            var levelId = BruTile.Utilities.GetNearestLevel(_tileSchema.Resolutions, _resolution);
            var tilesNeeded = _fetchStrategy.GetTilesWanted(_tileSchema, _extent.ToExtent(), levelId);
            NumberTilesNeeded = tilesNeeded.Count;
            var tileNeededNotInCacheOrInProgress = tilesNeeded.Where(t => _tileCache.Find(t.Index) == null && !_tilesInProgress.Contains(t.Index));
            _tilesMissing =  new ConcurrentQueue<TileInfo>(tileNeededNotInCacheOrInProgress.ToList());
            if (_tilesMissing.Count > 0) Busy = true;
        }
    }
}
