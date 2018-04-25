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
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Mapsui.Tests")]
namespace Mapsui.Fetcher
{
    class TileFetchDispatcher : IFetchDispatcher, INotifyPropertyChanged
    {
        private BoundingBox _extent;
        private double _resolution;
        private readonly object _lockRoot = new object();
        private bool _busy;
        private int _numberTilesNeeded;
        private bool _modified;
        private readonly ITileCache<Feature> _tileCache;
        private readonly IFetchStrategy _fetchStrategy;
        private ConcurrentQueue<TileInfo> _tilesMissing = new ConcurrentQueue<TileInfo>();
        private readonly ConcurrentHashSet<TileIndex> _tilesInProgress = new ConcurrentHashSet<TileIndex>();
        private ITileSource _tileSource;

        public TileFetchDispatcher(ITileCache<Feature> tileCache, IFetchStrategy fetchStrategy = null)
        {
            _tileCache = tileCache;
            _fetchStrategy = fetchStrategy ?? new MinimalFetchStrategy();
        }

        public event DataChangedEventHandler DataChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public int NumberTilesNeeded => _numberTilesNeeded;

        public ITileSource TileSource
        {
            get => _tileSource;
            set
            {
                _modified = true;
                _tileSource = value;
            }
        }

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
                var tileData = TileSource.GetTile(tileInfo);
                FetchCompleted(tileInfo, tileData, null);
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

        private void FetchCompleted(TileInfo tileInfo, byte[] tileData, Exception exception)
        {
            lock (_lockRoot)
            {
                if (exception == null)
                {
                    _tileCache.Add(tileInfo.Index, ToFeature(tileInfo, tileData));
                }
                _tilesInProgress.TryRemove(tileInfo.Index);

                Busy = _tilesInProgress.Count > 0 || _tilesMissing.Count > 0;

                DataChanged?.Invoke(this, new DataChangedEventArgs(exception, false, null));
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
            if (TileSource == null)
            {
                _tilesMissing = new ConcurrentQueue<TileInfo>();
                _tilesInProgress.Clear();
                return;
            }
            
            var levelId = BruTile.Utilities.GetNearestLevel(TileSource.Schema.Resolutions, _resolution);
            var tilesNeeded = _fetchStrategy.GetTilesWanted(TileSource.Schema, _extent.ToExtent(), levelId);
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
