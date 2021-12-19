using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BruTile;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Logging;
using Mapsui.Rendering;

namespace Mapsui.Layers
{
    public class RasterizingTileLayer : TileLayer
    {
        private readonly ILayer _layer;

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
        /// <param name="fetchTileAsFeature">Fetch tile as feature</param>
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
            Func<TileInfo, RasterFeature?>? fetchTileAsFeature = null)
            : base(new RasterizingTileProvider(layer, renderResolutionMultiplier, rasterizer, pixelDensity),
            minTiles,
            maxTiles,
            dataFetchStrategy,
            renderFetchStrategy,
            minExtraTiles,
            maxExtraTiles,
            fetchTileAsFeature)
        {
            _layer = layer;
        }

        public override void RefreshData(FetchInfo fetchInfo)
        {
            ((RasterizingTileProvider)TileSource).RefreshData(fetchInfo);
            _layer.RefreshData(fetchInfo);
            base.RefreshData(fetchInfo);
        }
    }
}