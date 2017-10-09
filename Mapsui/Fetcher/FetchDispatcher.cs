using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BruTile;
using BruTile.Cache;
using ConcurrentCollections;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Fetcher
{
    class FetchDispatcher : IFetchDispatcher, INotifyPropertyChanged
    {
        private readonly ITileCache<Feature> _tileCache;
        private readonly ITileSource _tileSource;
        private readonly IFetchStrategy _fetchStrategy;
        private bool _modified;
        private BoundingBox _extent;
        private double _resolution;
        private ConcurrentQueue<TileInfo> _tilesMissing = new ConcurrentQueue<TileInfo>();
        private readonly ConcurrentHashSet<TileIndex> _tilesInProgress = new ConcurrentHashSet<TileIndex>();
        private readonly object _lockRoot = new object();
        private bool _busy;
        private int _numberTilesNeeded;
        
        public FetchDispatcher(ITileCache<Feature> tileCache, ITileSource tileSource, IFetchStrategy fetchStrategy = null)
        {
            
            _tileCache = tileCache;
            _tileSource = tileSource;
            _fetchStrategy = fetchStrategy ?? new MinimalFetchStrategy();
        }

        public event DataChangedEventHandler DataChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public int NumberTilesNeeded => _numberTilesNeeded;


        public void SetViewport(BoundingBox newExtent, double newResolution)
        {
            lock (_lockRoot)
            {
                _extent = newExtent;
                _resolution = newResolution;
                Busy = true;
                _modified = true;
            }
        }

        public Action TakeFetchOrder()
        {
            lock (_lockRoot)
            {
                UpdateIfModified();
                TileInfo tileInfo;
                var success = _tilesMissing.TryDequeue(out tileInfo);

                if (success)
                {
                    _tilesInProgress.Add(tileInfo.Index);
                    return () => ExecuteOrder(tileInfo);
                }

                Busy = _tilesInProgress.Count > 0 || _tilesMissing.Count > 0;
                // else the queue is empty, we are done.
                return null; // return null to indicate we are done.
            }
        }

        private void ExecuteOrder(TileInfo tileInfo)
        {
            byte[] tileData = null;
            try
            {
                tileData = _tileSource.GetTile(tileInfo);
                CompleteFetchOrder(tileInfo, tileData, null);
            }
            catch (Exception exception)
            {
                CompleteFetchOrder(tileInfo, tileData, exception);
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

        public void CompleteFetchOrder(TileInfo tileInfo, byte[] tileData, Exception exception)
        {
            lock (_lockRoot)
            {
                if (exception == null)
                {
                    _tileCache.Add(tileInfo.Index, ToFeature(tileInfo, tileData));
                }
                _tilesInProgress.TryRemove(tileInfo.Index);

                Busy = _tilesInProgress.Count > 0 || _tilesMissing.Count > 0;

                DataChanged?.Invoke(this, new DataChangedEventArgs(null, false, null));
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

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateMissingTiles()
        {
            var levelId = BruTile.Utilities.GetNearestLevel(_tileSource.Schema.Resolutions, _resolution);
            var tilesNeeded = _fetchStrategy.GetTilesWanted(_tileSource.Schema, _extent.ToExtent(), levelId);
            _numberTilesNeeded = tilesNeeded.Count;
            var tileNeededNotInCacheOrInProgress = tilesNeeded.Where(t => _tileCache.Find(t.Index) == null && !_tilesInProgress.Contains(t.Index));
            _tilesMissing =  new ConcurrentQueue<TileInfo>(tileNeededNotInCacheOrInProgress.ToList());
            if (_tilesMissing.Count > 0) Busy = true;
        }

        private static Feature ToFeature(TileInfo tileInfo, byte[] tileData)
        {
            return new Feature { Geometry = ToGeometry(tileInfo, tileData) };
        }

        private static Raster ToGeometry(TileInfo tileInfo, byte[] tileData)
        {
            // A TileSource may return a byte array that is null. This is currently only implemented
            // for MbTilesTileSource. It is to indicate that the tile is not present in the source,
            // although it should be given the tile schema. It does not mean the tile could not
            // be accessed because of some temporary reason. In that case it will throw an exception.
            // For Mapsui this is important because it will not try again and again to fetch it. 
            // Here we return the geometry as null so that it will be added to the tile cache. 
            // TileLayer.GetFeatureInView will have to return only the non null geometries.

            if (tileData == null) return null;

            return new Raster(new MemoryStream(tileData), tileInfo.Extent.ToBoundingBox());
        }
    }
}
