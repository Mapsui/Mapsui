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

using BruTile;
using BruTile.Cache;
using BruTile.Tms;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Mapsui.Layers
{
    public interface ITileLayer
    {
        ITileSchema Schema { get; }
        MemoryCache<Feature> MemoryCache { get; }
    }

    public class TileLayer : BaseLayer, ITileLayer
    {
        private TileFetcher _tileFetcher;
        private ITileSource _tileSource;
        private readonly string _urlToTileMapXml;
        private readonly bool _overrideTmsUrlWithUrlToTileMapXml;
        private readonly int _maxRetries;
        private readonly int _maxThreads;
        private readonly IFetchStrategy _fetchStrategy;

        readonly MemoryCache<Feature> _memoryCache;

        private void LoadTmsLayer(IAsyncResult result)
        {
            var state = (object[])result.AsyncState;
            var initializationFailed = (Action<Exception>)state[1];
                
            try
            {
                var request = (HttpWebRequest) state[0];
                var response = request.EndGetResponse(result);
                var stream = response.GetResponseStream();
                SetTileSource(_overrideTmsUrlWithUrlToTileMapXml
                    ? TileMapParser.CreateTileSource(stream, _urlToTileMapXml)
                    : TileMapParser.CreateTileSource(stream));
            }
            catch (Exception ex)
            {
                if (initializationFailed != null)
                    initializationFailed(new Exception("Could not initialize TileLayer with url : " + _urlToTileMapXml, ex));
                // else: hopelesly lost with an error on a background thread with no option to report back.
            }
        }

        [Obsolete("use the named parameters of the constructor with tilesource if you want to omit the tilesource")]
        public TileLayer(int minTiles = 200, int maxTiles = 300, int maxRetries = TileFetcher.DefaultMaxRetries,
            int maxThreads = TileFetcher.DefaultMaxThreads, IFetchStrategy fetchStrategy = null)
        {
            _memoryCache = new MemoryCache<Feature>(minTiles, maxTiles);
            Style = new VectorStyle { Outline = { Color = Color.FromArgb(0, 0, 0, 0) } }; // initialize with transparent outline
            _maxRetries = maxRetries;
            _maxThreads = maxThreads;
            _fetchStrategy = fetchStrategy ?? new FetchStrategy();
        }

        public TileLayer(ITileSource source, int minTiles = 200, int maxTiles = 300, int maxRetries = TileFetcher.DefaultMaxRetries,
            int maxThreads = TileFetcher.DefaultMaxThreads, IFetchStrategy fetchStrategy = null)
            : this(minTiles, maxTiles, maxRetries, maxThreads, fetchStrategy)
        {
            SetTileSource(source);
        }

        [Obsolete("TileLayer should not have a dependency on TMS. This method will be removed")]
        public TileLayer(string urlToTileMapXml, bool overrideTmsUrlWithUrlToTileMapXml = false, Action<Exception> initializationFailed = null)
            : this()
        {
            _urlToTileMapXml = urlToTileMapXml;
            _overrideTmsUrlWithUrlToTileMapXml = overrideTmsUrlWithUrlToTileMapXml;
            var webRequest = (HttpWebRequest)WebRequest.Create(urlToTileMapXml);
            webRequest.BeginGetResponse(LoadTmsLayer, new object[] { webRequest, initializationFailed });
        }

        private void SetTileSource(ITileSource source)
        {
            _tileSource = source;
            _tileFetcher = new TileFetcher(source, _memoryCache, _maxRetries, _maxThreads, _fetchStrategy);
            _tileFetcher.DataChanged += TileFetcherDataChanged;
            OnPropertyChanged("Envelope");
        }
        
        public override BoundingBox Envelope
        {
            get
            {
                if (Schema == null) return null;
                return Schema.Extent.ToBoundingBox();
            }
        }

        public override void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            if (Enabled && extent.GetArea() > 0 && _tileFetcher != null)
            {
                _tileFetcher.ViewChanged(extent, resolution);
            }
        }

        /// <summary>
        /// Aborts the fetch of data that is currently in progress.
        /// With new ViewChanged calls the fetch will start again. 
        /// Call this method to speed up garbage collection
        /// </summary>
        public override void AbortFetch()
        {
            if (_tileFetcher != null)
            {
                _tileFetcher.AbortFetch();
            }
        }

        public override void ClearCache()
        {
            _memoryCache.Clear();
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
            get { return _tileSource != null ? _tileSource.Schema : null; }
        }

        public MemoryCache<Feature> MemoryCache
        {
            get { return _memoryCache; }
        }

        #endregion

        private void TileFetcherDataChanged(object sender, DataChangedEventArgs e)
        {
            OnDataChanged(e);
        }
        
        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var dictionary = new Dictionary<TileIndex, IFeature>();

            if (Schema == null) return dictionary.Values;

            var levelId = BruTile.Utilities.GetNearestLevel(Schema.Resolutions, resolution);
            GetRecursive(dictionary, Schema, _memoryCache, box.ToExtent(), levelId);
            var sortedDictionary = dictionary.OrderByDescending(t => Schema.Resolutions[t.Key.Level].UnitsPerPixel);
            return sortedDictionary.ToDictionary(pair => pair.Key, pair => pair.Value).Values;
        }

        public static void GetRecursive(IDictionary<TileIndex, IFeature> resultTiles, ITileSchema schema,
            MemoryCache<Feature> cache, Extent extent, string levelId)
        {
            var resolution = schema.Resolutions[levelId].UnitsPerPixel;
            var tiles = schema.GetTilesInView(extent, resolution);

            foreach (var tileInfo in tiles)
            {
                var feature = cache.Find(tileInfo.Index);
                var nextLevelId = schema.Resolutions.Where(r => r.Value.UnitsPerPixel > resolution)
                       .OrderBy(r => r.Value.UnitsPerPixel).FirstOrDefault().Key;
                   
                if (feature == null)
                {
                    if (nextLevelId != null) GetRecursive(resultTiles, schema, cache, tileInfo.Extent.Intersect(extent), nextLevelId);
                }
                else
                {
                    resultTiles[tileInfo.Index] = feature;
                    if (!IsFullyShown(feature))
                    {
                        if (nextLevelId != null) GetRecursive(resultTiles, schema, cache, tileInfo.Extent.Intersect(extent), nextLevelId);
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
