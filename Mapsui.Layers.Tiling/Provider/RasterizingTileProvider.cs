using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Rendering;

namespace Mapsui.Layers;

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
    private readonly ETileFormat _tileFormat;

    public RasterizingTileProvider(
        ILayer layer,
        double renderResolutionMultiplier = 1,
        IRenderer? rasterizer = null,
        float pixelDensity = 1,
        IPersistentCache<byte[]>? persistentCache = null,
        IProjection? projection = null,
        ETileFormat tileFormat = ETileFormat.Png)
    {
        _tileFormat = tileFormat;
        _layer = layer;
        _renderResolutionMultiplier = renderResolutionMultiplier;
        _rasterizer = rasterizer;
        _pixelDensity = pixelDensity;
        PersistentCache = persistentCache ?? new NullCache();

        if (_layer is IDataSourceLayer { DataSource: { } } dataSourceLayer)
        {
            _dataSource = dataSourceLayer.DataSource;
            if (!string.IsNullOrEmpty(_dataSource.CRS) && _dataSource.CRS != this.Schema.Srs)
            {
                // The TileSchema and the _dataSource.CRS are different Project it.
                _dataSource = new ProjectingProvider(_dataSource, projection ?? new Projection())
                {
                    CRS = this.Schema.Srs // The Schema SRS
                };
            }
        }
    }

    public IPersistentCache<byte[]> PersistentCache { get; set; }

    public byte[]? GetTile(TileInfo tileInfo)
    {
        var result = PersistentCache.Find(tileInfo.Index);
        if (result == null)
        {
            var renderer = GetRenderer();
            var viewPort = CreateRenderLayer(tileInfo, out var renderLayer);

            MemoryStream? stream = null;
            try
            {
                switch (_tileFormat)
                {
                    case ETileFormat.Png:
                        stream = renderer.RenderToBitmapStream(viewPort, new[] { renderLayer }, pixelDensity: _pixelDensity);
                        break;
                    case ETileFormat.Skp:
                        if (renderer is IPictureRenderer pictureRenderer)
                        {
                            stream = pictureRenderer.RenderToPictureStream(viewPort, new[] { renderLayer });
                        }
                        
                        break;
                }

                _rasterizingLayers.Push(renderer);
                result = stream?.ToArray();
                PersistentCache?.Add(tileInfo.Index, result ?? Array.Empty<byte>());
            }
            finally
            {
                stream?.Dispose();
            }
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
        renderLayer = new MemoryLayer(_layer.Name)
        {
            Style = _layer.Style,
            DataSource = new MemoryProvider<IFeature>(features),
            Attribution = _layer.Attribution,
            Opacity = _layer.Opacity,
        };
        return viewPort;
    }

    private IEnumerable<IFeature> GetFeatures(FetchInfo fetchInfo)
    {
        if (_dataSource != null)
        {
            return _dataSource.GetFeatures(fetchInfo);
        }

        return _layer.GetFeatures(fetchInfo.Extent, fetchInfo.Resolution);
    }

    private IRenderer GetRenderer()
    {
        if (!_rasterizingLayers.TryPop(out var rasterizer))
        {
            rasterizer = _rasterizer ?? DefaultRendererFactory.Create();
        }

        return rasterizer;
    }

    public ITileSchema Schema => _tileSchema ??= new GlobalSphericalMercator();
    public string Name => _layer.Name;
    public Attribution Attribution => _attribution ??= new Attribution(_layer.Attribution.Text, _layer.Attribution.Url);

    public object? GetPictureTile(TileInfo tileInfo)
    {
        var renderer = GetRenderer() as IPictureRenderer;

        if (renderer == null)
        {
            return null;
        }
        var viewPort = CreateRenderLayer(tileInfo, out var renderLayer);
        var result = renderer.RenderToPicture(viewPort, new[] { renderLayer });
        _rasterizingLayers.Push(renderer);
        return result;
    }
}