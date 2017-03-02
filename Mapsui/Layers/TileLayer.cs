// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
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

using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private TileFetcher _tileFetcher;
        private ITileSource _tileSource;
        private readonly int _maxRetries;
        private readonly int _maxThreads;
        private readonly IFetchStrategy _fetchStrategy;
        private readonly IRenderGetStrategy _renderFetchStrategy;
        private readonly int _minExtraTiles;
        private readonly int _maxExtraTiles;
        private int _numberTilesNeeded;

        readonly MemoryCache<Feature> _memoryCache;

        public TileLayer(Func<ITileSource> tileSourceInitializer) : this()
        {
            Task.Factory.StartNew(() => TileSource = tileSourceInitializer());
        }

        public TileLayer(ITileSource source = null, int minTiles = 200, int maxTiles = 300, int maxRetries = TileFetcher.DefaultMaxAttempts,
            int maxThreads = TileFetcher.DefaultMaxThreads, IFetchStrategy fetchStrategy = null,
            IRenderGetStrategy renderFetchStrategy = null, int minExtraTiles = -1, int maxExtraTiles = -1)
        {
            _memoryCache = new MemoryCache<Feature>(minTiles, maxTiles);
            Style = new VectorStyle { Outline = { Color = Color.FromArgb(0, 0, 0, 0) } }; // initialize with transparent outline
            _maxRetries = maxRetries;
            _maxThreads = maxThreads;
            _fetchStrategy = fetchStrategy ?? new FetchStrategy();
            _renderFetchStrategy = renderFetchStrategy ?? new RenderGetStrategy();
            _minExtraTiles = minExtraTiles;
            _maxExtraTiles = maxExtraTiles;
            SetTileSource(source);
        }

        protected void SetTileSource(ITileSource source)
        {
            if (_tileSource != null)
            {
                // Is causing thread leak _tileFetcher.AbortFetch();
                _tileFetcher.DataChanged -= TileFetcherDataChanged;
                _tileFetcher.PropertyChanged -= TileFetcherOnPropertyChanged;
                _tileFetcher = null;
                _memoryCache.Clear();
            }

            _tileSource = source;

            if (_tileSource != null)
            {
                Attribution.Text = _tileSource.Attribution?.Text;
                Attribution.Url = _tileSource.Attribution?.Url;
                _tileFetcher = new TileFetcher(source, _memoryCache, _maxRetries, _maxThreads, _fetchStrategy);
                _tileFetcher.DataChanged += TileFetcherDataChanged;
                _tileFetcher.PropertyChanged += TileFetcherOnPropertyChanged;
                OnPropertyChanged(nameof(Envelope));
            }
        }

        private void TileFetcherOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Busy))
            {
                if (_tileFetcher != null) Busy = _tileFetcher.Busy;
            }
        }

        public ITileSource TileSource
        {
            get { return _tileSource; }
            set
            {
                SetTileSource(value);
                OnPropertyChanged(nameof(TileSource));
            }
        }

        public override BoundingBox Envelope => Schema?.Extent.ToBoundingBox();

        public override void ViewChanged(bool majorChange, BoundingBox extent, double resolution)
        {
            if (Enabled && extent.GetArea() > 0 && _tileFetcher != null && MaxVisible > resolution && MinVisible < resolution)
            {
                _tileFetcher.ViewChanged(extent, resolution);
            }
        }

        private void UpdateMemoryCacheMinAndMax()
        {
            if (_minExtraTiles < 0 || _maxExtraTiles < 0 
                || _numberTilesNeeded == _tileFetcher.NumberTilesNeeded) return;
            _numberTilesNeeded = _tileFetcher.NumberTilesNeeded;
            _memoryCache.MinTiles = _numberTilesNeeded + _minExtraTiles;
            _memoryCache.MaxTiles = _numberTilesNeeded + _maxExtraTiles;
        }

        public override void ClearCache()
        {
            _memoryCache.Clear();
        }

        // TODO: 
        // investigate whether we can do without this public Schema. 
        // Its primary use is in the Renderer which recursively searches for
        // available tiles. Perhaps this recursive search can be done within
        // this class. I would be nice though if there was some flexibility into
        // the specific search strategy. Perhaps it is possible to pass a search 
        // to some GetTiles method.
        // Update. Schema is not used in the Renderer anymore and TileSource is now a public property
        public ITileSchema Schema => _tileSource?.Schema;

        public MemoryCache<Feature> MemoryCache => _memoryCache;

        private void TileFetcherDataChanged(object sender, DataChangedEventArgs e)
        {
            OnDataChanged(e);
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            if (Schema == null) return Enumerable.Empty<IFeature>();
            UpdateMemoryCacheMinAndMax();
            return _renderFetchStrategy.GetFeatures(box, resolution, Schema, _memoryCache);
        }

        public override bool? IsCrsSupported(string crs)
        {
            return (string.Equals(ToSimpleEpsgCode(), crs, StringComparison.CurrentCultureIgnoreCase));
        }

        string ToSimpleEpsgCode()
        {
            var startEpsgCode = TileSource.Schema.Srs.IndexOf("EPSG:", StringComparison.Ordinal);
            if (startEpsgCode < 0) return TileSource.Schema.Srs;
            return TileSource.Schema.Srs.Substring(startEpsgCode).Replace("::", ":").Trim();
        }

        public override void AbortFetch()
        {
            // to nothing for now
        }
    }
}