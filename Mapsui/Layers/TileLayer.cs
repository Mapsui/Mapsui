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
    public class TileLayer : BaseLayer
    {
        private TileFetcher _tileFetcher;
        private ITileSource _tileSource;
        private readonly int _maxRetries;
        private readonly int _maxThreads;
        private readonly IFetchStrategy _fetchStrategy;
        private readonly IRenderGetStrategy _renderStrategy;
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
            _renderStrategy = renderFetchStrategy ?? new RenderGetStrategy();
            _minExtraTiles = minExtraTiles;
            _maxExtraTiles = maxExtraTiles;
            SetTileSource(source);
        }

        private void SetTileSource(ITileSource source)
        {
            if (_tileSource != null)
            {
                AbortFetcher();
                _memoryCache.Clear();
            }

            _tileSource = source;

            if (_tileSource != null)
            {
                Attribution.Text = _tileSource.Attribution?.Text;
                Attribution.Url = _tileSource.Attribution?.Url;

                CreateFetcher(source);

                OnPropertyChanged(nameof(Envelope));
            }
        }

        private void CreateFetcher(ITileSource source)
        {
            _tileFetcher = new TileFetcher(source, _memoryCache, _maxRetries, _maxThreads, _fetchStrategy);
            _tileFetcher.DataChanged += TileFetcherDataChanged;
            _tileFetcher.PropertyChanged += TileFetcherOnPropertyChanged;
        }

        private void AbortFetcher()
        {
            if (_tileFetcher == null) return;
            var tileFetcher = _tileFetcher;
            _tileFetcher = null;
            tileFetcher.AbortFetch();
            tileFetcher.DataChanged -= TileFetcherDataChanged;
            tileFetcher.PropertyChanged -= TileFetcherOnPropertyChanged;
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
            get => _tileSource;
            set
            {
                SetTileSource(value);
                OnPropertyChanged(nameof(TileSource));
            }
        }

        public override BoundingBox Envelope => _tileSource?.Schema?.Extent.ToBoundingBox();

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

        public MemoryCache<Feature> MemoryCache => _memoryCache;

        private void TileFetcherDataChanged(object sender, DataChangedEventArgs e)
        {
            OnDataChanged(e);
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            if (_tileSource?.Schema == null) return Enumerable.Empty<IFeature>();
            UpdateMemoryCacheMinAndMax();
            return _renderStrategy.GetFeatures(box, resolution, _tileSource?.Schema, _memoryCache);
        }

        public override bool? IsCrsSupported(string crs)
        {
            return (string.Equals(ToSimpleEpsgCode(), crs, StringComparison.CurrentCultureIgnoreCase));
        }

        string ToSimpleEpsgCode()
        {
            var startEpsgCode = _tileSource.Schema.Srs.IndexOf("EPSG:", StringComparison.Ordinal);
            if (startEpsgCode < 0) return _tileSource.Schema.Srs;
            return _tileSource.Schema.Srs.Substring(startEpsgCode).Replace("::", ":").Trim();
        }

        // Aborts the tile fetch thread. When the fetcher thread is still running
        // the layer will not be disposed. Call this method only if the layer is 
        // not used anymore
        public override void AbortFetch()
        {
            AbortFetcher();
        }

        public override IReadOnlyList<double> Resolutions => _tileSource?.Schema?.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();
    }
}