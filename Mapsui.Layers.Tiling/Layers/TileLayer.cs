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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BruTile;
using BruTile.Cache;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Rendering;
using Mapsui.Styles;

#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.

namespace Mapsui.Layers
{
    /// <summary>
    /// Layer, which displays a map consisting of individual tiles
    /// </summary>
    public class TileLayer : BaseLayer, IAsyncDataFetcher
    {
        private readonly ITileSource _tileSource;
        private readonly IRenderFetchStrategy _renderFetchStrategy;
        private readonly int _minExtraTiles;
        private readonly int _maxExtraTiles;
        private int _numberTilesNeeded;
        private readonly TileFetchDispatcher _tileFetchDispatcher;
        private readonly MRect? _extent;

        /// <summary>
        /// Create tile layer for given tile source
        /// </summary>
        /// <param name="tileSource">Tile source to use for this layer</param>
        /// <param name="minTiles">Minimum number of tiles to cache</param>
        /// <param name="maxTiles">Maximum number of tiles to cache</param>
        /// <param name="dataFetchStrategy">Strategy to get list of tiles for given extent</param>
        /// <param name="renderFetchStrategy"></param>
        /// <param name="minExtraTiles">Number of minimum extra tiles for memory cache</param>
        /// <param name="maxExtraTiles">Number of maximum extra tiles for memory cache</param>
        /// <param name="fetchTileAsFeature">Fetch tile as feature</param>
        // ReSharper disable once UnusedParameter.Local // Is public and won't break this now
        public TileLayer(ITileSource tileSource, int minTiles = 200, int maxTiles = 300,
            IDataFetchStrategy? dataFetchStrategy = null, IRenderFetchStrategy? renderFetchStrategy = null,
            int minExtraTiles = -1, int maxExtraTiles = -1, Func<TileInfo, RasterFeature?>? fetchTileAsFeature = null)
        {
            _tileSource = tileSource ?? throw new ArgumentException($"{tileSource} can not null");
            MemoryCache = new MemoryCache<RasterFeature?>(minTiles, maxTiles);
            Style = new VectorStyle { Outline = { Color = Color.FromArgb(0, 0, 0, 0) } }; // initialize with transparent outline
            Attribution.Text = _tileSource.Attribution?.Text;
            Attribution.Url = _tileSource.Attribution?.Url;
            _extent = _tileSource.Schema?.Extent.ToMRect();
            dataFetchStrategy ??= new DataFetchStrategy(3);
            _renderFetchStrategy = renderFetchStrategy ?? new RenderFetchStrategy();
            _minExtraTiles = minExtraTiles;
            _maxExtraTiles = maxExtraTiles;
            _tileFetchDispatcher = new TileFetchDispatcher(MemoryCache, _tileSource.Schema, fetchTileAsFeature ?? ToFeature, dataFetchStrategy);
            _tileFetchDispatcher.DataChanged += TileFetchDispatcherOnDataChanged;
            _tileFetchDispatcher.PropertyChanged += TileFetchDispatcherOnPropertyChanged;
        }

        /// <summary>
        /// TileSource</summary>
        public ITileSource TileSource => _tileSource;

        /// <summary>
        /// Memory cache for this layer
        /// </summary>
        private MemoryCache<RasterFeature?> MemoryCache { get; }

        /// <inheritdoc />
        public override IReadOnlyList<double> Resolutions => _tileSource.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();

        /// <inheritdoc />
        public override MRect? Extent => _extent;

        /// <inheritdoc />
        public override IEnumerable<IFeature> GetFeatures(MRect extent, double resolution)
        {
            if (_tileSource.Schema == null) return Enumerable.Empty<IFeature>();
            UpdateMemoryCacheMinAndMax();
            return _renderFetchStrategy.Get(extent, resolution, _tileSource.Schema, MemoryCache);
        }

        /// <inheritdoc />
        public void AbortFetch()
        {
            _tileFetchDispatcher.StopFetching();
        }

        /// <inheritdoc />
        public void ClearCache()
        {
            MemoryCache.Clear();
        }

        /// <inheritdoc />
        public override void RefreshData(FetchInfo fetchInfo)
        {
            if (Enabled
                && fetchInfo.Extent?.GetArea() > 0
                && MaxVisible >= fetchInfo.Resolution
                && MinVisible <= fetchInfo.Resolution)
            {
                _tileFetchDispatcher.SetViewport(fetchInfo);
                _tileFetchDispatcher.StartFetching();
            }
        }

        private void TileFetchDispatcherOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Busy))
            {
                Busy = _tileFetchDispatcher.Busy;
            }
        }

        private void UpdateMemoryCacheMinAndMax()
        {
            if (_minExtraTiles < 0 || _maxExtraTiles < 0) return;
            if (_numberTilesNeeded == _tileFetchDispatcher.NumberTilesNeeded) return;

            _numberTilesNeeded = _tileFetchDispatcher.NumberTilesNeeded;
            MemoryCache.MinTiles = _numberTilesNeeded + _minExtraTiles;
            MemoryCache.MaxTiles = _numberTilesNeeded + _maxExtraTiles;
        }

        private void TileFetchDispatcherOnDataChanged(object sender, DataChangedEventArgs e)
        {
            OnDataChanged(e);
        }

        private RasterFeature? ToFeature(TileInfo tileInfo)
        {
            var tileData = _tileSource.GetTile(tileInfo);
            var mRaster = ToRaster(tileInfo, tileData);
            if (mRaster != null)
                return new RasterFeature(mRaster);

            return null;
        }

        private static MRaster? ToRaster(TileInfo tileInfo, byte[]? tileData)
        {
            // A TileSource may return a byte array that is null. This is currently only implemented
            // for MbTilesTileSource. It is to indicate that the tile is not present in the source,
            // although it should be given the tile schema. It does not mean the tile could not
            // be accessed because of some temporary reason. In that case it will throw an exception.
            // For Mapsui this is important because it will not try again and again to fetch it. 
            // Here we return the geometry as null so that it will be added to the tile cache. 
            // TileLayer.GetFeatureInView will have to return only the non null geometries.

            if (tileData == null) return null;

            return new MRaster(new MemoryStream(tileData), tileInfo.Extent.ToMRect());
        }
    }
}
