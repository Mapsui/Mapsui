// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System.Net;
using BruTile;
using BruTile.Cache;
using BruTile.Web.TmsService;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using System.Collections.Generic;
using Mapsui.Providers;
using Mapsui.Styles;
using System;
using System.Linq;

namespace Mapsui.Layers
{
    public interface ITileLayer
    {
        ITileSchema Schema { get; }
        MemoryCache<Feature> MemoryCache { get; }
    }

    public class TileLayer : BaseLayer, ITileLayer
    {
        private TileFetcher tileFetcher;
        private ITileSource tileSource;
        private readonly string urlToTileMapXml;
        private readonly bool overrideTmsUrlWithUrlToTileMapXml;
        private int maxRetries;

        public int MaxRetries
        {
            set { maxRetries = value; }
        } 

        readonly MemoryCache<Feature> memoryCache = new MemoryCache<Feature>(200, 300);

        private void LoadTmsLayer(IAsyncResult result)
        {
            var state = (object[])result.AsyncState;
            var initializationFailed = (Action<Exception>)state[1];
                
            try
            {
                var request = (HttpWebRequest) state[0];
                var response = request.EndGetResponse(result);
                var stream = response.GetResponseStream();
                SetTileSource(overrideTmsUrlWithUrlToTileMapXml
                    ? TileMapParser.CreateTileSource(stream, urlToTileMapXml)
                    : TileMapParser.CreateTileSource(stream));
                Style = new VectorStyle();
            }
            catch (Exception ex)
            {
                if (initializationFailed != null)
                    initializationFailed(new Exception("Could not initialize TileLayer with url : " + urlToTileMapXml, ex));
                // else: hopelesly lost with an error on a background thread with no option to report back.
            }
        }

        public TileLayer(string urlToTileMapXml, bool overrideTmsUrlWithUrlToTileMapXml = false, Action<Exception> initializationFailed = null)
        {
            this.urlToTileMapXml = urlToTileMapXml;
            this.overrideTmsUrlWithUrlToTileMapXml = overrideTmsUrlWithUrlToTileMapXml;
            var webRequest = (HttpWebRequest)WebRequest.Create(urlToTileMapXml);
            webRequest.BeginGetResponse(LoadTmsLayer, new object[] { webRequest, initializationFailed });
        }
        
        public TileLayer(ITileSource source)
            : this()
        {
            SetTileSource(source);
        }

        protected void SetTileSource(ITileSource source)
        {
            tileSource = source;
            tileFetcher = new TileFetcher(source, memoryCache, maxRetries);
            tileFetcher.DataChanged += TileFetcherDataChanged;
            OnPropertyChanged("Envelope");
        }

        public TileLayer()
        {
            // We need to add a style on the layer or else all features will
            // be ignored altogher. Perhaps the style on the individual Features
            // should be used instead. Perhaps the Feature.Style could contain 
            // the tile data iso the Feature.Geometry
            Style = new VectorStyle();
        }

        public override BoundingBox Envelope
        {
            get
            {
                if (Schema == null) return null;
                return Schema.Extent.ToBoundingBox();
            }
        }

        public override event DataChangedEventHandler DataChanged;

        public override void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            if (Enabled && extent.GetArea() > 0 && tileFetcher != null)
            {
                tileFetcher.ViewChanged(extent, resolution);
            }
        }

        /// <summary>
        /// Aborts the fetch of data that is currently in progress.
        /// With new ViewChanged calls the fetch will start again. 
        /// Call this method to speed up garbage collection
        /// </summary>
        public override void AbortFetch()
        {
            if (tileFetcher != null)
            {
                tileFetcher.AbortFetch();
            }
        }

        public override void ClearCache()
        {
            memoryCache.Clear();
        }

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
            get { return tileSource != null ? tileSource.Schema : null; }
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

            if (Schema == null) return dictionary.Values;

            GetRecursive(dictionary, Schema, memoryCache, box.ToExtent(), BruTile.Utilities.GetNearestLevel(Schema.Resolutions, resolution));
            var sortedDictionary = (from entry in dictionary orderby entry.Key ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
            return sortedDictionary.Values;
        }

        public static void GetRecursive(IDictionary<TileIndex, IFeature> resultTiles, ITileSchema schema, MemoryCache<Feature> cache, Extent extent, int level)
        {
            if (level < 0) return;

            var tiles = schema.GetTilesInView(extent, level);

            foreach (TileInfo tileInfo in tiles)
            {
                var feature = cache.Find(tileInfo.Index);
                if (feature == null)
                {
                    GetRecursive(resultTiles, schema, cache, tileInfo.Extent.Intersect(extent), level - 1);
                }
                else
                {
                    resultTiles[tileInfo.Index] = feature;
                    if (!IsFullyShown(feature))
                    {
                        GetRecursive(resultTiles, schema, cache, tileInfo.Extent.Intersect(extent), level - 1);
                    }
                }
            }
        }

        public static bool IsFullyShown(Feature feature)
        {
            var currentTile = DateTime.Now.Ticks;
            var tile = ((IRaster)feature.Geometry);
            const long second = 10000000;
            return ((currentTile - tile.TickFetched) > second);
        }
    }
}
