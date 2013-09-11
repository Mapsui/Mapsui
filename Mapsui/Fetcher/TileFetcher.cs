// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BruTile;
using BruTile.Cache;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Fetcher
{
    class TileFetcher
    {
        #region Fields

        private readonly MemoryCache<Feature> memoryCache;
        private readonly ITileSource tileSource;
        private BoundingBox extent;
        private double resolution;
        private readonly IList<TileIndex> tilesInProgress = new List<TileIndex>();
        private IList<TileInfo> missingTiles = new List<TileInfo>();
        private readonly IDictionary<TileIndex, int> retries = new Dictionary<TileIndex, int>();
        private const int ThreadMax = 2;
        private int threadCount;
        private readonly AutoResetEvent waitHandle = new AutoResetEvent(true);
        private readonly IFetchStrategy strategy = new FetchStrategy();
        private readonly int maxRetries = 2;
        private volatile bool isThreadRunning;
        private volatile bool isViewChanged;
        
        #endregion

        #region EventHandlers

        public event DataChangedEventHandler DataChanged;

        #endregion

        #region Constructors Destructors

        public TileFetcher(ITileSource tileSource, MemoryCache<Feature> memoryCache, int maxRetries = 2)
        {
            if (tileSource == null) throw new ArgumentException("TileProvider can not be null");
            this.tileSource = tileSource;

            if (memoryCache == null) throw new ArgumentException("MemoryCache can not be null");
            this.memoryCache = memoryCache;

            this.maxRetries = maxRetries;
        }

        #endregion

        #region Public Methods

        public void ViewChanged(BoundingBox newExtent, double newResolution)
        {
            extent = newExtent;
            resolution = newResolution;
            isViewChanged = true;
            waitHandle.Set();
            if (!isThreadRunning) { StartLoopThread(); }
        }

        private void StartLoopThread()
        {
            isThreadRunning = true;
            ThreadPool.QueueUserWorkItem(TileFetchLoop);
        }

        public void AbortFetch()
        {
            isThreadRunning = false;
            waitHandle.Set();
        }

        #endregion

        #region Private Methods

        private void TileFetchLoop(object state)
        {
            try
            {
                while (isThreadRunning)
                {
                    if (tileSource.Schema == null) waitHandle.Reset();

                    waitHandle.WaitOne();

                    if (isViewChanged && (tileSource.Schema != null))
                    {
                        int level = BruTile.Utilities.GetNearestLevel(tileSource.Schema.Resolutions, resolution);
                        missingTiles = strategy.GetTilesWanted(tileSource.Schema, extent.ToExtent(), level);
                        retries.Clear();
                        isViewChanged = false;
                    }

                    missingTiles = GetTilesMissing(missingTiles, memoryCache, retries, maxRetries);

                    FetchTiles();

                    if (missingTiles.Count == 0) { waitHandle.Reset(); }

                    if (threadCount >= ThreadMax) { waitHandle.Reset(); }
                }
            }
            finally
            {
                isThreadRunning = false;
            }
        }

        private static IList<TileInfo> GetTilesMissing(IEnumerable<TileInfo> infosIn, MemoryCache<Feature> memoryCache, IDictionary<TileIndex, int> retries, int maxRetries)
        {
            IList<TileInfo> tilesOut = new List<TileInfo>();
            foreach (TileInfo info in infosIn)
            {
                if ((memoryCache.Find(info.Index) == null) &&
                    (!retries.Keys.Contains(info.Index) || retries[info.Index] < maxRetries))

                    tilesOut.Add(info);
            }
            return tilesOut;
        }

        private void FetchTiles()
        {
            foreach (TileInfo info in missingTiles)
            {
                if (threadCount >= ThreadMax) return;
                FetchTile(info);
            }
        }

        private void FetchTile(TileInfo info)
        {
            //first a number of checks
            if (tilesInProgress.Contains(info.Index)) return;
            if (retries.Keys.Contains(info.Index) && retries[info.Index] >= maxRetries) return;
            if (memoryCache.Find(info.Index) != null) return;

            //now we can go for the request.
            lock (tilesInProgress) { tilesInProgress.Add(info.Index); }
            if (!retries.Keys.Contains(info.Index)) retries.Add(info.Index, 0);
            else retries[info.Index]++;
            threadCount++;
            StartFetchOnThread(info);
        }

        private void StartFetchOnThread(TileInfo info)
        {
            var fetchOnThread = new FetchOnThread(tileSource.Provider, info, LocalFetchCompleted);
            ThreadPool.QueueUserWorkItem(fetchOnThread.FetchTile);
        }

        private void LocalFetchCompleted(object sender, FetchTileCompletedEventArgs e)
        {
            //todo remove object sender
            try
            {
                if (e.Error == null && e.Cancelled == false && isThreadRunning && e.Image != null)
                {
                    var feature = new Feature
                        {
                            Geometry = new Raster(new MemoryStream(e.Image), e.TileInfo.Extent.ToBoundingBox())
                        };
                    memoryCache.Add(e.TileInfo.Index, feature);
                }
            }
            catch (Exception ex)
            {
                e.Error = ex;
            }
            finally
            {
                threadCount--;
                lock (tilesInProgress)
                {
                    if (tilesInProgress.Contains(e.TileInfo.Index))
                        tilesInProgress.Remove(e.TileInfo.Index);
                }
                waitHandle.Set();
            }

            if (DataChanged != null)
                DataChanged(this, new DataChangedEventArgs(e.Error, e.Cancelled, e.TileInfo));
        }

        #endregion
    }
}
