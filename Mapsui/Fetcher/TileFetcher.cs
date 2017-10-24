// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using Mapsui.Annotations;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Fetcher
{
    [Obsolete("Use TileFetchDispatcher and FetchMachine")]
    public class TileFetcher : INotifyPropertyChanged
    {
        public const int DefaultMaxThreads = 2;
        public const int DefaultMaxAttempts = 2;
        private readonly MemoryCache<Feature> _memoryCache;
        private readonly ITileSource _tileSource;
        private BoundingBox _extent;
        private double _resolution;
        private readonly IList<TileIndex> _tilesInProgress = new List<TileIndex>();
        private IList<TileInfo> _missingTiles = new List<TileInfo>();
        private readonly int _maxThreads;
        private int _threadCount;
        private readonly AutoResetEvent _waitHandle = new AutoResetEvent(true);
        private readonly IFetchStrategy _strategy;
        private readonly int _maxAttempts;
        private volatile bool _isThreadRunning;
        private volatile bool _isViewChanged;
        private bool _busy;
        private int _numberTilesNeeded;

        public event DataChangedEventHandler DataChanged;

        public TileFetcher(ITileSource tileSource, MemoryCache<Feature> memoryCache, int maxAttempts = DefaultMaxAttempts, int maxThreads = DefaultMaxThreads, IFetchStrategy strategy = null)
        {
            if (tileSource == null) throw new ArgumentException("TileProvider can not be null");
            if (memoryCache == null) throw new ArgumentException("MemoryCache can not be null");

            _tileSource = tileSource;
            _memoryCache = memoryCache;
            _maxAttempts = maxAttempts;
            _maxThreads = maxThreads;
            _strategy = strategy ?? new FetchStrategy();
        }

        public bool Busy
        {
            get { return _busy; }
            private set
            {
                if (_busy == value) return; // prevent notify              
                _busy = value;
                OnPropertyChanged("Busy");
            }
        }

        public int NumberTilesNeeded => _numberTilesNeeded;


        public void ViewChanged(BoundingBox newExtent, double newResolution)
        {
            _extent = newExtent;
            _resolution = newResolution;
            _isViewChanged = true;
            _waitHandle.Set();

            if (!_isThreadRunning)
            {
                StartLoopThread();
                Busy = true;
            }
        }

        private void StartLoopThread()
        {
            _isThreadRunning = true;
            Task.Run(() => TileFetchLoop());
        }

        public void AbortFetch()
        {
            _isThreadRunning = false;
            _waitHandle.Set();
        }

        private void TileFetchLoop()
        {
            try
            {
                var retries = new Retries(_maxAttempts);

                while (_isThreadRunning)
                {
                    if (_tileSource.Schema == null) _waitHandle.Reset();

                    _waitHandle.WaitOne();
                    Busy = true;

                    if (_isViewChanged && (_tileSource.Schema != null))
                    {
                        var levelId = BruTile.Utilities.GetNearestLevel(_tileSource.Schema.Resolutions, _resolution);
                        _missingTiles = _strategy.GetTilesWanted(_tileSource.Schema, _extent.ToExtent(), levelId);
                        _numberTilesNeeded = _missingTiles.Count;
                        retries.Clear();
                        _isViewChanged = false;
                    }

                    _missingTiles = GetTilesMissing(_missingTiles, _memoryCache, retries);

                    FetchTiles(retries);

                    if (_missingTiles.Count == 0)
                    {
                        Busy = false;
                        _waitHandle.Reset();
                    }

                    if (_threadCount >= _maxThreads) { _waitHandle.Reset(); }
                }
            }
            finally
            {
                _isThreadRunning = false;
            }
        }

        private IList<TileInfo> GetTilesMissing(IEnumerable<TileInfo> tileInfos, MemoryCache<Feature> memoryCache,
            Retries retries)
        {
            var result = new List<TileInfo>();

            foreach (var info in tileInfos)
            {
                if (retries.ReachedMax(info.Index)) continue;
                if (memoryCache.Find(info.Index) == null) result.Add(info);
            }

            return result;
        }

        private void FetchTiles(Retries retries)
        {
            foreach (var info in _missingTiles)
            {
                if (_threadCount >= _maxThreads) break;
                FetchTile(info, retries);
            }
        }

        private void FetchTile(TileInfo info, Retries retries)
        {
            if (retries.ReachedMax(info.Index)) return;

            lock (_tilesInProgress)
            {
                if (_tilesInProgress.Contains(info.Index)) return;
                _tilesInProgress.Add(info.Index);
            }

            retries.PlusOne(info.Index);
            _threadCount++;

            StartFetchOnThread(info);
        }

        private void StartFetchOnThread(TileInfo info)
        {
            var fetchOnThread = new FetchOnThread(_tileSource, info, LocalFetchCompleted);
            Task.Run(() => fetchOnThread.FetchTile());
        }

        private void LocalFetchCompleted(object sender, FetchTileCompletedEventArgs e)
        {
            //todo remove object sender
            try
            {
                if (e.Error == null && e.Cancelled == false && _isThreadRunning)
                {
                    var geometry = CreateTileGeometry(e);
                    var feature = new Feature { Geometry = geometry };
                    _memoryCache.Add(e.TileInfo.Index, feature);
                }
            }
            catch (Exception ex)
            {
                e.Error = ex;
            }
            finally
            {
                _threadCount--;
                lock (_tilesInProgress)
                {
                    if (_tilesInProgress.Contains(e.TileInfo.Index))
                        _tilesInProgress.Remove(e.TileInfo.Index);
                }
                _waitHandle.Set();
            }

            DataChanged?.Invoke(this, new DataChangedEventArgs(e.Error, e.Cancelled, e.TileInfo));
        }

        private static Raster CreateTileGeometry(FetchTileCompletedEventArgs e)
        {
            // A TileSource may return an byte array that is null. This is currently only implemented
            // for MbTilesTileSource. It is to indicate that the tile is not present in the source,
            // although it should be given the tile schema. It does not mean the tile could not
            // be accessed because of some temporary reason. In that case it will throw an exception.
            // For Mapsui this is important because it will not try again and again to fetch it. 
            // Here we return the geometry as null so that it will be added to the tile cache. 
            // TileLayer.GetFeatureInView will have to return only the non null geometries.

            if (e.Image == null) return null;

            return new Raster(new MemoryStream(e.Image), e.TileInfo.Extent.ToBoundingBox());
        }


        /// <summary>
        /// Keeps track of retries per tile. This class doesn't do much interesting work
        /// but makes the rest of the code a bit easier to read.
        /// </summary>
        class Retries
        {
            private readonly IDictionary<TileIndex, int> _retries = new Dictionary<TileIndex, int>();
            private readonly int _maxRetries;
            private readonly int _threadId;
            private const string CrossThreadExceptionMessage = "Cross thread access not allowed on class Retries";

            public Retries(int maxRetries)
            {
                _maxRetries = maxRetries;
                _threadId = Environment.CurrentManagedThreadId;
            }

            public bool ReachedMax(TileIndex index)
            {
                if (_threadId != Environment.CurrentManagedThreadId) throw new Exception(CrossThreadExceptionMessage);

                var retryCount = (!_retries.Keys.Contains(index)) ? 0 : _retries[index];
                return retryCount > _maxRetries;
            }

            public void PlusOne(TileIndex index)
            {
                if (_threadId != Environment.CurrentManagedThreadId) throw new Exception(CrossThreadExceptionMessage);

                if (!_retries.Keys.Contains(index)) _retries.Add(index, 0);
                else _retries[index]++;
            }

            public void Clear()
            {
                if (_threadId != Environment.CurrentManagedThreadId) throw new Exception(CrossThreadExceptionMessage);

                _retries.Clear();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
