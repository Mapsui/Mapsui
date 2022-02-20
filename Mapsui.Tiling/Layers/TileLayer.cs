// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BruTile;
using BruTile.Cache;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Tiling.Extensions;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Rendering;

// ReSharper disable once VirtualMemberCallInConstructor
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.

namespace Mapsui.Tiling.Layers
{
    /// <summary>
    /// Layer, which displays a map consisting of individual tiles
    /// </summary>
    public class TileLayer : BaseLayer, IAsyncDataFetcher, IDisposable
    {
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
            int minExtraTiles = -1, int maxExtraTiles = -1, Func<TileInfo, IFeature?>? fetchTileAsFeature = null)
        {
            TileSource = tileSource ?? throw new ArgumentException($"{tileSource} can not null");
            MemoryCache = new MemoryCache<IFeature?>(minTiles, maxTiles);
            Style = new VectorStyle { Outline = { Color = Color.FromArgb(0, 0, 0, 0) } }; // initialize with transparent outline
            Attribution.Text = TileSource.Attribution?.Text;
            Attribution.Url = TileSource.Attribution?.Url;
            Extent = TileSource.Schema?.Extent.ToMRect();
            dataFetchStrategy ??= new DataFetchStrategy(3);
            RenderFetchStrategy = renderFetchStrategy ?? new RenderFetchStrategy();
            MinExtraTiles = minExtraTiles;
            MaxExtraTiles = maxExtraTiles;
            TileFetchDispatcher = new TileFetchDispatcher(MemoryCache, TileSource.Schema, fetchTileAsFeature ?? ToFeature, dataFetchStrategy);
            TileFetchDispatcher.DataChanged += TileFetchDispatcherOnDataChanged;
            TileFetchDispatcher.PropertyChanged += TileFetchDispatcherOnPropertyChanged;
        }

        /// <summary>
        /// Max Extra Tiles
        /// </summary>
        public int MaxExtraTiles { get; protected set; }

        /// <summary>
        /// Min Extra Tiles
        /// </summary>
        public int MinExtraTiles { get; protected set; }

        /// <summary>
        /// Render Fetch Strategy
        /// </summary>
        public IRenderFetchStrategy RenderFetchStrategy { get; protected set; }

        /// <summary>
        /// TileSource
        /// </summary>
        public ITileSource TileSource { get; protected set; }

        /// <summary>
        /// Tile Fetch Dispatcher
        /// </summary>
        protected TileFetchDispatcher TileFetchDispatcher { get; set; }

        /// <summary>
        /// Number Tiles Needed
        /// </summary>
        protected int NumberTilesNeeded { get; set; }

        /// <summary>
        /// Memory cache for this layer
        /// </summary>
        protected MemoryCache<IFeature?> MemoryCache { get; }

        /// <inheritdoc />
        public override IReadOnlyList<double> Resolutions => TileSource.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();

        /// <inheritdoc />
        public override MRect? Extent { get; protected set; }

        /// <inheritdoc />
        public override IEnumerable<IFeature> GetFeatures(MRect extent, double resolution)
        {
            if (TileSource.Schema == null) return Enumerable.Empty<IFeature>();
            UpdateMemoryCacheMinAndMax();
            return RenderFetchStrategy.Get(extent, resolution, TileSource.Schema, MemoryCache);
        }

        /// <inheritdoc />
        public void AbortFetch()
        {
            TileFetchDispatcher.StopFetching();
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
                TileFetchDispatcher.SetViewport(fetchInfo);
                TileFetchDispatcher.StartFetching();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                MemoryCache.Dispose();

            base.Dispose(disposing);
        }

        protected virtual void TileFetchDispatcherOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Busy))
                Busy = TileFetchDispatcher.Busy;
        }

        protected virtual void UpdateMemoryCacheMinAndMax()
        {
            if (MinExtraTiles < 0 || MaxExtraTiles < 0) return;
            if (NumberTilesNeeded == TileFetchDispatcher.NumberTilesNeeded) return;

            NumberTilesNeeded = TileFetchDispatcher.NumberTilesNeeded;
            MemoryCache.MinTiles = NumberTilesNeeded + MinExtraTiles;
            MemoryCache.MaxTiles = NumberTilesNeeded + MaxExtraTiles;
        }

        protected virtual void TileFetchDispatcherOnDataChanged(object sender, DataChangedEventArgs e)
        {
            OnDataChanged(e);
        }

        private RasterFeature? ToFeature(TileInfo tileInfo)
        {
            var tileData = TileSource.GetTile(tileInfo);
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
            return new MRaster(tileData, tileInfo.Extent.ToMRect());
        }
    }
}
