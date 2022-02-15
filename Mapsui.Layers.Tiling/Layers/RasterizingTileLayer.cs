using System;
using System.Collections.Generic;
using System.ComponentModel;
using BruTile;
using BruTile.Cache;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Projections;
using Mapsui.Rendering;
using Mapsui.Widgets;

namespace Mapsui.Layers
{
    public class RasterizingTileLayer : BaseLayer, ISourceLayer, IAsyncDataFetcher
    {
        private readonly RasterizingTileProvider _tileProvider;
        private readonly TileLayer _tileLayer;

        /// <summary>
        ///     Creates a RasterizingTileLayer which rasterizes a layer for performance
        /// </summary>
        /// <param name="layer">The Layer to be rasterized</param>
        /// <param name="renderResolutionMultiplier"></param>
        /// <param name="rasterizer">Rasterizer to use. null will use the default</param>
        /// <param name="pixelDensity"></param>
        /// <param name="minTiles">Minimum number of tiles to cache</param>
        /// <param name="maxTiles">Maximum number of tiles to cache</param>
        /// <param name="dataFetchStrategy">Strategy to get list of tiles for given extent</param>
        /// <param name="renderFetchStrategy"></param>
        /// <param name="minExtraTiles">Number of minimum extra tiles for memory cache</param>
        /// <param name="maxExtraTiles">Number of maximum extra tiles for memory cache</param>
        /// <param name="persistentCache">Persistent Cache</param>
        public RasterizingTileLayer(
            ILayer layer,
            double renderResolutionMultiplier = 1,
            IRenderer? rasterizer = null,
            float pixelDensity = 1,
            int minTiles = 200,
            int maxTiles = 300,
            IDataFetchStrategy? dataFetchStrategy = null,
            IRenderFetchStrategy? renderFetchStrategy = null,
            int minExtraTiles = -1,
            int maxExtraTiles = -1,
            IPersistentCache<byte[]>? persistentCache = null,
            IProjection? projection = null)
        {
            _tileProvider = new RasterizingTileProvider(layer, renderResolutionMultiplier, rasterizer, pixelDensity, persistentCache, projection);
            _tileLayer = new TileLayer(_tileProvider,
                minTiles,
                maxTiles,
                dataFetchStrategy,
                renderFetchStrategy,
                minExtraTiles,
                maxExtraTiles);
            _tileLayer.DataChanged += TileLayerDataChanged;
            _tileLayer.PropertyChanged += TileLayerPropertyChanged;
            SourceLayer = layer;
        }

        /// <inheritdoc />
        public override IReadOnlyList<double> Resolutions => _tileLayer.Resolutions;

        /// <inheritdoc />
        public override MRect? Extent => _tileLayer.Extent;

        /// <inheritdoc />
        public override bool Busy => _tileLayer.Busy;

        /// <inheritdoc />
        public override Hyperlink Attribution => _tileLayer.Attribution;

        /// <inheritdoc />
        public override IEnumerable<IFeature> GetFeatures(MRect extent, double resolution)
        {
            return _tileLayer.GetFeatures(extent, resolution);
        }

        /// <inheritdoc />
        public override void RefreshData(FetchInfo fetchInfo)
        {
            _tileLayer.RefreshData(fetchInfo);
        }

        private void TileLayerDataChanged(object sender, DataChangedEventArgs e)
        {
            OnDataChanged(e);
        }

        private void TileLayerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public ILayer SourceLayer { get; }

        public void AbortFetch()
        {
            _tileLayer.AbortFetch();
        }

        public void ClearCache()
        {
            _tileLayer.ClearCache();
        }
    }
}