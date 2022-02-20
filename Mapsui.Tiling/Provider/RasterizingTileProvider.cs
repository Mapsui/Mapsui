using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Tiling.Extensions;

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
    private readonly IProvider<IFeature>? _dataSource;

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

        if (_layer is ILayerDataSource { DataSource: { } } dataSourceLayer)
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

    public byte[]? GetTile(TileInfo tileInfo)
    {
        var index = tileInfo.Index;
        var result = PersistentCache.Find(index);
        if (result == null)
        {
            var renderer = GetRenderer();
            var viewPort = CreateRenderLayer(tileInfo, out var renderLayer);

            using var stream = renderer.RenderToBitmapStream(viewPort, new[] { renderLayer }, pixelDensity: _pixelDensity);;
            _rasterizingLayers.Push(renderer);
            result = stream?.ToArray();
            PersistentCache?.Add(index, result ?? Array.Empty<byte>());
        }

        return result;
    }

    private Viewport CreateRenderLayer(TileInfo tileInfo, out ILayer renderLayer)
    {
        Schema.Resolutions.TryGetValue(tileInfo.Index.Level, out var tileResolution);

        var resolution = tileResolution.UnitsPerPixel;
        var viewPort = RasterizingLayer.CreateViewport(tileInfo.Extent.ToMRect(), resolution, _renderResolutionMultiplier, 1);
        var fetchInfo = new FetchInfo(viewPort.Extent, resolution);
        var features = GetFeatures(fetchInfo);
        renderLayer = new RenderLayer(_layer, features);
        return viewPort;
    }

    private IEnumerable<IFeature> GetFeatures(FetchInfo fetchInfo)
    {
        if (_dataSource != null) return _dataSource.GetFeatures(fetchInfo);

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