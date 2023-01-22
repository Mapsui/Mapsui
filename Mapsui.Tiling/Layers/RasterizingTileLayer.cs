using System.Collections.Generic;
using System.ComponentModel;
using BruTile.Cache;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Rendering;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Provider;
using Mapsui.Tiling.Rendering;
using Mapsui.Widgets;

namespace Mapsui.Tiling.Layers;

public class RasterizingTileLayer : TileLayer, ISourceLayer, IAsyncDataFetcher
{
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
    /// <param name="projection">Projection</param>
    /// <param name="renderFormat">Format to Render To</param>
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
        IProjection? projection = null,
        RenderFormat renderFormat = RenderFormat.Png) : base(
        new RasterizingTileProvider(layer, renderResolutionMultiplier, rasterizer, pixelDensity, persistentCache, projection, renderFormat),
        minTiles,
        maxTiles,
        dataFetchStrategy,
        renderFetchStrategy,
        minExtraTiles,
        maxExtraTiles)
    {
        SourceLayer = layer;
    }

    public ILayer SourceLayer { get; }
}
