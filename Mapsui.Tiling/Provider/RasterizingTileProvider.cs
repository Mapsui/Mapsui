using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using Attribution = BruTile.Attribution;

namespace Mapsui.Tiling.Provider;

/// <summary> The rasterizing tile provider. Tiles the Layer for faster Rasterizing on Zoom and Move. </summary>
public class RasterizingTileProvider : ITileSource
{
    private readonly ConcurrentStack<IRenderer> _rasterizingLayers = new();
    private readonly IRenderer? _rasterizer;
    private readonly float _pixelDensity;
    private readonly ILayer _layer;
    private ITileSchema? _tileSchema;
    private Attribution? _attribution;
    private readonly IProvider? _dataSource;
    private readonly RenderFormat _renderFormat;
    private readonly IDictionary<TileIndex, double> _searchSizeCache = new ConcurrentDictionary<TileIndex, double>();

    public RasterizingTileProvider(
        ILayer layer,
        IRenderer? rasterizer = null,
        float pixelDensity = 1,
        IPersistentCache<byte[]>? persistentCache = null,
        IProjection? projection = null,
        RenderFormat renderFormat = RenderFormat.Png)
    {
        _renderFormat = renderFormat;
        _layer = layer;
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
            (MSection section, ILayer renderLayer) = await CreateRenderLayerAsync(tileInfo, renderer);
            using var stream = renderer.RenderToBitmapStream(ToViewport(section), new[] { renderLayer }, pixelDensity: _pixelDensity, renderFormat: _renderFormat);
            _rasterizingLayers.Push(renderer);
            result = stream?.ToArray();
            PersistentCache?.Add(index, result ?? Array.Empty<byte>());
        }

        return result;
    }

    private async Task<(MSection section, ILayer RenderLayer)> CreateRenderLayerAsync(TileInfo tileInfo, IRenderer renderer)
    {
        var indexLevel = tileInfo.Index.Level;
        Schema.Resolutions.TryGetValue(indexLevel, out var tileResolution);

        var resolution = tileResolution.UnitsPerPixel;
        var section = new MSection(tileInfo.Extent.ToMRect(), resolution);
        var featureSearchGrowth = await GetAdditionalSearchSizeAroundAsync(tileInfo, renderer, section);
        var extent = section.Extent;
        if (featureSearchGrowth > 0)
        {
            // do not expand beyond the bounds of the Schema fixes not loading data in Because of invalid bounds
            var minX = extent.MinX - featureSearchGrowth;
            var minY = extent.MinY - featureSearchGrowth;
            var maxX = extent.MaxX + featureSearchGrowth;
            var maxY = extent.MaxY + featureSearchGrowth;

            var schemaExtent = Schema.Extent;
            if (minX < schemaExtent.MinX) minX = schemaExtent.MinX;
            if (minY < schemaExtent.MinY) minY = schemaExtent.MinY;
            if (maxX > schemaExtent.MaxX) maxX = schemaExtent.MaxX;
            if (maxY > schemaExtent.MaxY) maxY = schemaExtent.MaxY;

            extent = new MRect(minX, minY, maxX, maxY);
        }

        var fetchInfo = new FetchInfo(new MSection(extent, resolution));
        var features = await GetFeaturesAsync(fetchInfo);
        var renderLayer = new RenderLayer(_layer, features);
        return (section, renderLayer);
    }

    private async Task<IEnumerable<IFeature>> GetFeaturesAsync(TileInfo tileInfo)
    {
        Schema.Resolutions.TryGetValue(tileInfo.Index.Level, out var tileResolution);

        var resolution = tileResolution.UnitsPerPixel;
        var section = new MSection(tileInfo.Extent.ToMRect(), resolution);
        var fetchInfo = new FetchInfo(section);
        var features = await GetFeaturesAsync(fetchInfo);
        return features;
    }

    private async Task<double> GetAdditionalSearchSizeAroundAsync(TileInfo tileInfo, IRenderer renderer, MSection section)
    {
        double additionalSearchSize = 0;

        for (int col = -1; col <= 1; col++)
        {
            for (int row = -1; row <= 1; row++)
            {
                var size = await GetAdditionalSearchSizeAsync(CreateTileInfo(tileInfo, col, row), renderer, section);
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

    private async Task<double> GetAdditionalSearchSizeAsync(TileInfo tileInfo, IRenderer renderer, MSection section)
    {
        if (!_searchSizeCache.TryGetValue(tileInfo.Index, out var result))
        {
            double tempSize = 0;

            var layerStyles = _layer.Style.GetStylesToApply(section.Resolution);
            foreach (var style in layerStyles)
            {
                if (renderer.StyleRenderers.TryGetValue(style.GetType(), out var styleRenderer))
                {
                    if (styleRenderer is IFeatureSize featureSize)
                    {
                        if (featureSize.NeedsFeature)
                        {
                            var features = await GetFeaturesAsync(tileInfo);
                            foreach (var feature in features)
                            {
                                tempSize = Math.Max(tempSize, featureSize.FeatureSize(style, renderer.RenderCache, feature));
                            }
                        }
                        else
                        {
                            tempSize = featureSize.FeatureSize(style, renderer.RenderCache, null);
                        }
                    }
                }
            }

            result = ConvertToCoordinates(tempSize, section.Resolution);
            _searchSizeCache[tileInfo.Index] = result;
        }

        return result;
    }

    private double ConvertToCoordinates(double tempSize, double resolution)
    {
        return tempSize * resolution * 0.5; // I need to load half the Size more of the Features
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

    public static Viewport ToViewport(MSection section)
    {
        return new Viewport(
            section.Extent.Centroid.X,
            section.Extent.Centroid.Y,
            section.Resolution,
            0,
            section.ScreenWidth,
            section.ScreenHeight);
    }
}
