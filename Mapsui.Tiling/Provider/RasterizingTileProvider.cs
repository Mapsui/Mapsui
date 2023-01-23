using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Tiling.Extensions;
using Mapsui.Utilities;
using Attribution = BruTile.Attribution;

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
    private readonly RenderFormat _renderFormat;
    private readonly AsyncLock _renderLock = new();
    private IDictionary<TileIndex, double> _searchSizeCache = new ConcurrentDictionary<TileIndex, double>();

    public RasterizingTileProvider(
        ILayer layer,
        double renderResolutionMultiplier = 1,
        IRenderer? rasterizer = null,
        float pixelDensity = 1,
        IPersistentCache<byte[]>? persistentCache = null,
        IProjection? projection = null,
        RenderFormat renderFormat = RenderFormat.Png)
    {
        _renderFormat = renderFormat;
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
            var renderer = GetRenderer();
            (Viewport viewPort, ILayer renderLayer) = await CreateRenderLayerAsync(tileInfo, renderer);
            using var stream = renderer.RenderToBitmapStream(viewPort, new[] { renderLayer }, pixelDensity: _pixelDensity, renderFormat: _renderFormat);
            _rasterizingLayers.Push(renderer);
            result = stream?.ToArray();
            PersistentCache?.Add(index, result ?? Array.Empty<byte>());
        }

        return result;
    }

    private async Task<(Viewport ViewPort, ILayer RenderLayer)> CreateRenderLayerAsync(TileInfo tileInfo, IRenderer renderer)
    {
        Schema.Resolutions.TryGetValue(tileInfo.Index.Level, out var tileResolution);

        var resolution = tileResolution.UnitsPerPixel;
        var viewPort = RasterizingLayer.CreateViewport(tileInfo.Extent.ToMRect(), resolution, _renderResolutionMultiplier, 1);
        var featureSearchGrowth = await GetAdditionalSearchSizeAroundAsync(tileInfo, renderer, viewPort);
        var extent = viewPort.Extent;
        if (featureSearchGrowth > 0)
        {
            extent = extent.Grow(featureSearchGrowth);
        }

        var fetchInfo = new FetchInfo(extent, resolution);
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

    private async Task<double> GetAdditionalSearchSizeAroundAsync(TileInfo tileInfo, IRenderer renderer, IReadOnlyViewport viewport)
    {
        double additionalSearchSize = 0;

        for (int col = -1; col <= 1; col++)
        {
            for (int row = -1; row <= 1; row++)
            {
                var size = await GetAdditionalSearchSizeAsync(CreateTileInfo(tileInfo, col, row), renderer, viewport);
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

    private async Task<double> GetAdditionalSearchSizeAsync(TileInfo tileInfo, IRenderer renderer, IReadOnlyViewport viewport)
    {
        if (!_searchSizeCache.TryGetValue(tileInfo.Index, out var result))
        {
            result = 0;
            var features = await GetFeaturesAsync(tileInfo);
            var layers = new List<ILayer> { new RenderLayer(_layer, features) };

            void MeasureFeature(IStyle style, IFeature feature)
            {
                var tempSize = GetFeatureSize(feature, style, renderer);
                var coordinateTempSize = ConvertToCoordinates(tempSize, viewport);
                result = Math.Max(coordinateTempSize, result);
            }

            VisibleFeatureIterator.IterateLayers(viewport, layers, 0, (v, l, s, f, o, i) =>
            {
                MeasureFeature(s, f);
            });

            _searchSizeCache[tileInfo.Index] = result;
        }

        return result;
    }

    private double ConvertToCoordinates(double tempSize, IReadOnlyViewport viewport)
    {
        return tempSize * viewport.Resolution * 0.5; // I need to load half the Size more of the Features
    }

    private double GetFeatureSize(IFeature feature, IStyle style, IRenderer renderer)
    {
        double size = 0;

        if (renderer.StyleRenderers.TryGetValue(style.GetType(), out var styleRenderer))
        {
            if (styleRenderer is IFeatureSize featureSize)
            {
                var tempSize = featureSize.FeatureSize(feature, style, renderer.RenderCache);
                size = Math.Max(tempSize, size);
            }
        }

        return size;
    }

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
