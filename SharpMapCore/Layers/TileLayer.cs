// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using BruTile;
using BruTile.Cache;
using SharpMap.Fetcher;
using SharpMap.Geometries;
using System.IO;
using System.Collections.Generic;
using SharpMap.Providers;
using SharpMap.Styles;
using System;
using System.Linq;

namespace SharpMap.Layers
{
    public interface ITileLayer
    {
        ITileSchema Schema { get; }
        MemoryCache<Feature> MemoryCache { get; }
    }

    public class TileLayer : BaseLayer, ITileLayer, IAsyncDataFetcher
    {
        readonly TileFetcher tileFetcher;
        readonly ITileSource tileSource;

#if PocketPC
        readonly MemoryCache<Feature> memoryCache = new MemoryCache<Feature>(40, 60);
#else
        readonly MemoryCache<Feature> memoryCache = new MemoryCache<Feature>(200, 300);
#endif

        public TileLayer(ITileSource source)
        {
            Enabled = true; //default enabled
            MinVisible = double.MinValue;
            MaxVisible = double.MaxValue;
            LayerName = "Layer";
            Opacity = 0.5;
            Styles.Add(new VectorStyle()); //TODO: Create a style which could be a default for all layers

            tileSource = source;
            tileFetcher = new TileFetcher(source, memoryCache);
            tileFetcher.DataChanged += TileFetcherDataChanged;
        }

        public override BoundingBox Envelope 
        {
            get 
            { 
                if (Schema == null) return null;
                return Schema.Extent.ToBoundingBox();
            }
        }

        #region IAsyncDataFetcher Members

        public event DataChangedEventHandler DataChanged;

        public void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            if (Enabled && extent.GetArea() > 0)
            {
                tileFetcher.ViewChanged(extent, resolution);
            }
        }

        /// <summary>
        /// Aborts the fetch of data that is currently in progress.
        /// With new ViewChanged calls the fetch will start again. 
        /// Call this method to speed up garbage collection
        /// </summary>
        public void AbortFetch()
        {
            if (tileFetcher != null)
            {
                tileFetcher.AbortFetch();
            }
        }

        public void ClearCache()
        {
            memoryCache.Clear();
        }

        #endregion

        #region ITileLayer Members

        public ITileSchema Schema
        {
            // TODO: 
            // investigate whether we can do without this public Schema. 
            // Its primary use is in the Renderer which recursively searches for
            // available tiles. Perhaps this recursive search can be done within
            // this class. I would be nice though if there was some flexibility into
            // the specific search strategy. Perhaps it is possible to pass a search 
            // to some GetTiles method.
            get { return tileSource.Schema; }
        }

        public MemoryCache<Feature> MemoryCache
        {
            get { return memoryCache; }
        }

        #endregion

        private void TileFetcherDataChanged(object sender, DataChangedEventArgs e)
        {
            OnDataChanged(e);
        }

        private void OnDataChanged(DataChangedEventArgs e)
        {
            if (DataChanged != null) DataChanged(this, e);
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var dictionary = new Dictionary<TileIndex, IFeature>();
            GetRecursive(dictionary, Schema, memoryCache, box.ToExtent(), BruTile.Utilities.GetNearestLevel(Schema.Resolutions, resolution));
            var sortedDictionary = (from entry in dictionary orderby entry.Key ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
            return sortedDictionary.Values;
        }

        private void GetRecursive(IDictionary<TileIndex, IFeature> resultTiles, ITileSchema schema, MemoryCache<Feature> memoryCache, Extent extent, int level)
        {
            if (level < 0) return;

            var tiles = schema.GetTilesInView(extent, level);
            
            foreach (TileInfo tileInfo in tiles)
            {
                var feature = memoryCache.Find(tileInfo.Index);
                if (feature == null)
                {
                    GetRecursive(resultTiles, schema, memoryCache, tileInfo.Extent.Intersect(extent), level - 1);
                }
                else
                {
                    resultTiles[tileInfo.Index] = feature;
                    if (!IsFullyShown(feature))
                    {
                        GetRecursive(resultTiles, schema, memoryCache, tileInfo.Extent.Intersect(extent), level - 1);
                    }
                }
            }
        }

        private bool IsFullyShown(Feature feature)
        {
            var currentTile = DateTime.Now.Ticks;
            var tile = ((IRaster)feature.Geometry);
            long second = 10000000;
            return ((currentTile - tile.TickFetched) > second);
        }
    }
}
