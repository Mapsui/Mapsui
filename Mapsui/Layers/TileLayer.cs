// TODO: There are parts talking about SharpMap

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
using Mapsui.Logging;
using Mapsui.Widgets;

namespace Mapsui.Layers
{
    /// <summary>
    /// Layer, which displays a map consisting of individual tiles
    /// </summary>
    public class TileLayer : BaseLayer, IAsyncDataFetcher
    {
        private ITileSource _tileSource;
        private readonly IRenderGetStrategy _renderGetStrategy;
        private readonly int _minExtraTiles;
        private readonly int _maxExtraTiles;
        private int _numberTilesNeeded;
        private readonly TileFetchDispatcher _tileFetchDispatcher;
        private readonly FetchMachine _fetchMachine;

        /// <summary>
        /// Create tile layer from tile source initializer function
        /// </summary>
        /// <param name="tileSourceInitializer">Initializer to create a tile layer source</param>
        public TileLayer(Func<ITileSource> tileSourceInitializer) : this()
        {
            Task.Run(() =>
            {
                try
                {
                    SetTileSource(tileSourceInitializer());
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Debug, $"Initialization of layer {Name} failed: {e.Message}");
                }
            });
        }

        /// <summary>
        /// Create tile layer for given tile source
        /// </summary>
        /// <param name="source">Tile source to use for this layer</param>
        /// <param name="minTiles">Minimum number of tiles to cache</param>
        /// <param name="maxTiles">Maximum number of tiles to cache</param>
        /// <param name="maxRetries">Unused</param>
        /// <param name="fetchStrategy">Strategy to get list of tiles for given extent</param>
        /// <param name="renderGetStrategy"></param>
        /// <param name="minExtraTiles">Number of minimum extra tiles for memory cache</param>
        /// <param name="maxExtraTiles">Number of maximum extra tiles for memory cache</param>
        // ReSharper disable once UnusedParameter.Local // Is public and won't break this now
        public TileLayer(ITileSource source = null, int minTiles = 200, int maxTiles = 300, int maxRetries = 2, IFetchStrategy fetchStrategy = null,
            IRenderGetStrategy renderGetStrategy = null, int minExtraTiles = -1, int maxExtraTiles = -1)
        {
            MemoryCache = new MemoryCache<Feature>(minTiles, maxTiles);
            Style = new VectorStyle { Outline = { Color = Color.FromArgb(0, 0, 0, 0) } }; // initialize with transparent outline
            var fetchStrategy1 = fetchStrategy ?? new FetchStrategy(3);
            _renderGetStrategy = renderGetStrategy ?? new RenderGetStrategy();
            _minExtraTiles = minExtraTiles;
            _maxExtraTiles = maxExtraTiles;
            _tileFetchDispatcher = new TileFetchDispatcher(MemoryCache, fetchStrategy1);
            _tileFetchDispatcher.DataChanged += TileFetchDispatcherOnDataChanged;
            _tileFetchDispatcher.PropertyChanged += TileFetchDispatcherOnPropertyChanged;
            _fetchMachine = new FetchMachine(_tileFetchDispatcher);
            SetTileSource(source);
        }

        /// <summary>
        /// Tile source for this layer
        /// </summary>
        public ITileSource TileSource
        {
            get => _tileSource;
            set
            {
                SetTileSource(value);
                OnPropertyChanged(nameof(TileSource));
            }
        }

        /// <summary>
        /// Memory cache for this layer
        /// </summary>
        private MemoryCache<Feature> MemoryCache { get; }

        /// <inheritdoc />
        public override IReadOnlyList<double> Resolutions => _tileSource?.Schema?.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();

        /// <inheritdoc />
        public override BoundingBox Envelope => _tileSource?.Schema?.Extent.ToBoundingBox();

        /// <inheritdoc />
        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            if (_tileSource?.Schema == null) return Enumerable.Empty<IFeature>();
            UpdateMemoryCacheMinAndMax();
            return _renderGetStrategy.GetFeatures(box, resolution, _tileSource?.Schema, MemoryCache);
        }

        /// <inheritdoc />
        public void AbortFetch()
        {
            _fetchMachine?.Stop();
        }

        /// <inheritdoc />
        public void ClearCache()
        {
            MemoryCache.Clear();
        }

        /// <inheritdoc />
        public override void RefreshData(BoundingBox extent, double resolution, bool majorChange)
        {
            if (Enabled && extent.GetArea() > 0 && _tileFetchDispatcher != null && MaxVisible >= resolution && MinVisible <= resolution)
            {
                _tileFetchDispatcher.SetViewport(extent, resolution);
                _fetchMachine.Start();
            }
        }

        /// <inheritdoc />
        public override bool? IsCrsSupported(string crs)
        {
            return (string.Equals(ToSimpleEpsgCode(), crs, StringComparison.CurrentCultureIgnoreCase));
        }

        private void SetTileSource(ITileSource tileSource)
		{
            _fetchMachine.Stop();
			MemoryCache.Clear();
		    _tileFetchDispatcher.TileSource = tileSource;
            _tileSource = tileSource;
            
            if (_tileSource != null)
            {
                if (Attribution == null) Attribution = new Hyperlink();
                Attribution.Text = _tileSource.Attribution?.Text;
                Attribution.Url = _tileSource.Attribution?.Url;
            }

		    OnPropertyChanged(nameof(Layer.DataSource)); // To trigger new RefreshData.
            OnPropertyChanged(nameof(Envelope));
        }

        private void TileFetchDispatcherOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Busy))
            {
                if (_tileFetchDispatcher != null) Busy = _tileFetchDispatcher.Busy;
            }
        }

        private void UpdateMemoryCacheMinAndMax()
        {
            if (_minExtraTiles < 0 || _maxExtraTiles < 0 
                || _numberTilesNeeded == _tileFetchDispatcher.NumberTilesNeeded) return;
            _numberTilesNeeded = _tileFetchDispatcher.NumberTilesNeeded;
            MemoryCache.MinTiles = _numberTilesNeeded + _minExtraTiles;
            MemoryCache.MaxTiles = _numberTilesNeeded + _maxExtraTiles;
        }

        private void TileFetchDispatcherOnDataChanged(object sender, DataChangedEventArgs e)
        {
            OnDataChanged(e);
        }

        private string ToSimpleEpsgCode()
        {
            var startEpsgCode = _tileSource.Schema.Srs.IndexOf("EPSG:", StringComparison.Ordinal);
            if (startEpsgCode < 0) return _tileSource.Schema.Srs;
            return _tileSource.Schema.Srs.Substring(startEpsgCode).Replace("::", ":").Trim();
        }
    }
}
