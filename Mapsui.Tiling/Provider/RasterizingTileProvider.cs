using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Tiling.Extensions;
using NeoSmart.AsyncLock;

namespace Mapsui.Tiling.Provider;

/// <summary> The rasterizing tile provider. Tiles the Layer for faster Rasterizing on Zoom and Move. </summary>
public class RasterizingTileProvider : ITileSource
{
    private readonly ConcurrentStack<IRenderer> _rasterizingLayers = new();
    private readonly double _renderResolutionMultiplier;
    private readonly IRenderer? _rasterizer;
    private readonly float _pixelDensity;
    private readonly ILayer _layer;
    private ITileSchema? _tileSchema;
    private Attribution? _attribution;
    private readonly IProvider? _dataSource;
    private readonly AsyncLock _renderLock = new();
    private Dictionary<TileIndex, double> _searchSizeCache = new ();

    public RasterizingTileProvider(
        ILayer layer,
        double renderResolutionMultiplier = 1,
        IRenderer? rasterizer = null,
        float pixelDensity = 1,
        IPersistentCache<byte[]>? persistentCache = null,
        IProjection? projection = null)
    {
        _layer = layer;
        _renderResolutionMultiplier = renderResolutionMultiplier;
        _rasterizer = rasterizer;
        _pixelDensity = pixelDensity;
        PersistentCache = persistentCache ?? new NullCache();

        if (_layer is ILayerDataSource<IProvider> { DataSource: { } } dataSourceLayer)
        {
            _dataSource = dataSourceLayer.DataSource;

            // The TileSchema and the _dataSource.CRS are different Project it.
            if (!string.IsNullOrEmpty(_dataSource.CRS) && _dataSource.CRS != Schema.Srs)
                _dataSource = new ProjectingProvider(_dataSource, projection)
                {
                    CRS = Schema.Srs // The Schema SRS
                };
        }
    }

    public IPersistentCache<byte[]> PersistentCache { get; set; }

    public async Task<byte[]?> GetTileAsync(TileInfo tileInfo)
    {
        var index = tileInfo.Index;
        var result = PersistentCache.Find(index);
        if (result == null)
        {
            MemoryStream? stream = null;

            try 
            { 
                using (await _renderLock.LockAsync())
                {
                    var renderer = GetRenderer();
                    (Viewport viewPort, ILayer renderLayer) = await CreateRenderLayerAsync(tileInfo, renderer);
                    try
                    {
                        stream = renderer.RenderToBitmapStream(viewPort, new[] { renderLayer }, pixelDensity: _pixelDensity);
                    }
                    finally
                    {
                        renderLayer.Dispose();
                    }
                    
                    _rasterizingLayers.Push(renderer);
                }

                result = stream?.ToArray();
            }
            finally
            {
                stream?.Dispose();
            }

            PersistentCache?.Add(index, result ?? Array.Empty<byte>());
        }

        return result;
    }

    private async Task<(Viewport ViewPort, ILayer RenderLayer)> CreateRenderLayerAsync(TileInfo tileInfo, IRenderer renderer)
    {
        Schema.Resolutions.TryGetValue(tileInfo.Index.Level, out var tileResolution);

        var resolution = tileResolution.UnitsPerPixel;
        var viewPort = RasterizingLayer.CreateViewport(tileInfo.Extent.ToMRect(), resolution, _renderResolutionMultiplier, 1);
        var featureSearchGrowth = await GetAdditionalSearchSizeAround(tileInfo, renderer);
        var extentGrown = viewPort.Extent.Grow(viewPort.Extent.Width + featureSearchGrowth);
        var fetchInfo = new FetchInfo(extentGrown, resolution); 
        var features = await GetFeaturesAsync(fetchInfo);
        var renderLayer = new RenderLayer(_layer, features);
        return (viewPort, renderLayer);
    }

    private async Task<IEnumerable<IFeature>> GetFeaturesAsync(TileInfo tileInfo)
    {
        Schema.Resolutions.TryGetValue(tileInfo.Index.Level, out var tileResolution);

        var resolution = tileResolution.UnitsPerPixel;
        var viewPort = RasterizingLayer.CreateViewport(tileInfo.Extent.ToMRect(), resolution, _renderResolutionMultiplier, 1);
        var fetchInfo = new FetchInfo(viewPort.Extent, resolution); 
        var features = await GetFeaturesAsync(fetchInfo);
        return features;
    }

    private async Task<double> GetAdditionalSearchSizeAround(TileInfo tileInfo, IRenderer renderer)
    {
        double additionalSearchSize = 0;
        
        for (int col = -1; col <= 1 ; col++)
        {
            for (int row = -1; row <= 1 ; row++)
            {
                var size = await GetAdditionalSearchSize(CreateTileInfo(tileInfo, col, row), renderer);
                additionalSearchSize = Math.Max(additionalSearchSize, size);
            }
        }

        return additionalSearchSize;
    }

    private TileInfo CreateTileInfo(TileInfo tileInfo, int col, int row)
    {
        var tileIndex = new TileIndex(tileInfo.Index.Col + col, tileInfo.Index.Row + row, tileInfo.Index.Level);
        var tileExtent = new Extent(
            tileInfo.Extent.MinX + tileInfo.Extent.Width * col,
            tileInfo.Extent.MinY + tileInfo.Extent.Height * row,
            tileInfo.Extent.MaxX + tileInfo.Extent.Width * col,
            tileInfo.Extent.MaxY + tileInfo.Extent.Height * row);
        return new TileInfo
        {
            Index = tileIndex,
            Extent = tileExtent,
        };
    }

    private async Task<double> GetAdditionalSearchSize(TileInfo tileInfo, IRenderer renderer)
    {
        if (!_searchSizeCache.TryGetValue(tileInfo.Index, out var result))
        {
            result = 0;
            var features = await GetFeaturesAsync(tileInfo);
            foreach (var feature in features)
            {
                var tempSize = GetFeatureSize(feature, renderer);
                var coordinateTempSize = ConvertToCoordinates(tempSize, tileInfo);
                result = Math.Max(coordinateTempSize, result);
            }
            
            _searchSizeCache[tileInfo.Index] = result;
        }

        return result;
    }

    private double GetFeatureSize(IFeature feature, IRenderer renderer)
    {
        var size = 0;
        foreach (var style in feature.Styles)
        {
            if (renderer.StyleRenderers.TryGetValue(style.GetType(), out var styleRenderer))
            {
                var featureSize = styleRenderer.FeatureSize(feature, style);
                size = Math.Max(featureSize, size);
            }
        }
        
        return size;
    }

    public ISymbolCache SymbolCache { get; set; }

    private async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        if (_dataSource != null)
        {
            return await _dataSource.GetFeaturesAsync(fetchInfo);
        }

        return _layer.GetFeatures(fetchInfo.Extent, fetchInfo.Resolution);
    }

    private IRenderer GetRenderer()
    {
        if (!_rasterizingLayers.TryPop(out var rasterizer)) rasterizer = _rasterizer ?? DefaultRendererFactory.Create();
        return rasterizer;
    }

    public ITileSchema Schema => _tileSchema ??= new GlobalSphericalMercator();
    public string Name => _layer.Name;
    public Attribution Attribution => _attribution ??= new Attribution(_layer.Attribution.Text, _layer.Attribution.Url);
}