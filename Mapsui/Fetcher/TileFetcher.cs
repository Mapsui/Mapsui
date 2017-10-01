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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using Mapsui.Annotations;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Fetcher
{
    public class TileFetcher : INotifyPropertyChanged
    {
        public const int DefaultMaxAttempts = 2;
        private readonly MemoryCache<Feature> _memoryCache;
        private readonly ITileSource _tileSource;
        private BoundingBox _extent;
        private double _resolution;
        private readonly IList<TileIndex> _tilesInProgress = new List<TileIndex>();
        private readonly IFetchStrategy _strategy;
        private volatile bool _viewportChanged;
        private bool _busy;
        private int _numberTilesNeeded;
		private Task _fetchLoopTask;
        private CancellationTokenSource _fetchLoopCancellationTokenSource;
        private readonly int _maxRequests = 32;
        private int _currentRequests;
        //private readonly EventWaitHandle _maxRequestsWaitHandle = new AutoResetEvent(true);
   

        public event DataChangedEventHandler DataChanged;

		public TileFetcher (ITileSource tileSource, MemoryCache<Feature> memoryCache, int maxRetries, int maxThreads, IFetchStrategy strategy = null)
		{
		    _tileSource = tileSource ?? throw new ArgumentException ("TileProvider can not be null");
			_memoryCache = memoryCache ?? throw new ArgumentException ("MemoryCache can not be null");
			_strategy = strategy ?? new FetchStrategy();
		}

        public bool Busy
        {
            get => _busy;
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
            _viewportChanged = true;

			if (_fetchLoopTask == null || _fetchLoopTask.IsCompleted || _fetchLoopCancellationTokenSource.IsCancellationRequested)
			{
				Busy = true;
				_fetchLoopCancellationTokenSource = new CancellationTokenSource();
				var fetchLoopCancellationToken = _fetchLoopCancellationTokenSource.Token;
				_fetchLoopTask = Task.Run (() => TileFetchLoop(fetchLoopCancellationToken), fetchLoopCancellationToken);
			}
        }

		public void AbortFetch()
		{
			_fetchLoopCancellationTokenSource?.Cancel();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void TileFetchLoop(CancellationToken globalCancellationToken)
        {
            List<TileInfo> tilesMissing = null;

            if (_tileSource.Schema == null) Busy = false;

		    while (Busy && !globalCancellationToken.IsCancellationRequested)
		    {
		        // 1) If the viewport is changed you need to calculate the needed tiles again.
		        if (_viewportChanged)
		        {
		            var tilesNeeded = GetTilesNeeded();
                    // assign needed tiles to missing tiles and over every iteration
                    // missing tiles will be smaller. This has a big impact on performance.
		            tilesMissing = tilesNeeded;
                    _viewportChanged = false;
		        }

		        // 2) From the needed tiles get those that are still missing
		        tilesMissing = GetMissingTiles(tilesMissing);

		        // 3) Check if we are done
		        if (!tilesMissing.Any()) { Busy = false; }
                
                // 4) Actually fetch the tiles missing.
                if (_currentRequests <  _maxRequests)
		            FetchMissingTiles(tilesMissing, globalCancellationToken);
		    }
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FetchMissingTiles(List<TileInfo> missingTiles, CancellationToken globalCancellationToken)
        {
            foreach (var info in missingTiles)
            {
                if (_currentRequests >= _maxRequests)
                {
                    //_maxRequestsWaitHandle.Stop();
                    //_maxRequestsWaitHandle.WaitOne();
                }
                lock (_tilesInProgress)
                {
                    if (_tilesInProgress.Contains(info.Index)) return;
                    _tilesInProgress.Add(info.Index);
                }

                _currentRequests++;
                // Not passing the cancellation token to Task.Run. 
                // Cancelling because of a viewport change can be ineffective because most of the tiles
                // will still be useful, even if they are outside of the viewport at that moment.
                // When abort is called we would like to cancel.
                Task.Run(() => FetchTile(info), globalCancellationToken);
                if (_viewportChanged) return;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<TileInfo> GetMissingTiles(List<TileInfo> tilesNeeded)
        {
            var missingTiles = new List<TileInfo>();

            foreach (var info in tilesNeeded)
            {
                if (_memoryCache.Find(info.Index) == null) missingTiles.Add(info);
            }
            return missingTiles;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<TileInfo> GetTilesNeeded()
        {
            var levelId = BruTile.Utilities.GetNearestLevel(_tileSource.Schema.Resolutions, _resolution);
            var tilesNeeded = _strategy.GetTilesWanted(_tileSource.Schema, _extent.ToExtent(), levelId).ToList();
            _numberTilesNeeded = tilesNeeded.Count;
            return tilesNeeded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FetchTile(TileInfo tileInfo)
        {
            try
            {
                var tileData = _tileSource.GetTile(tileInfo);
                var geometry = CreateTileGeometry(tileInfo, tileData);
                var feature = new Feature {Geometry = geometry};
                _memoryCache.Add(tileInfo.Index, feature);
                DataChanged?.Invoke(this, new DataChangedEventArgs(null, false, null));
            }
            catch (Exception ex)
            {
                DataChanged?.Invoke(this, new DataChangedEventArgs(ex, false, null));
            }
            finally
            {
                lock (_tilesInProgress)
                {
                    if (_tilesInProgress.Contains(tileInfo.Index))
                        _tilesInProgress.Remove(tileInfo.Index);
                }
                _currentRequests--;
                //if (_currentRequests < _maxRequests) _maxRequestsWaitHandle.Go();
            }

        }

        private static Raster CreateTileGeometry(TileInfo tileInfo, byte[] tileData)
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
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
