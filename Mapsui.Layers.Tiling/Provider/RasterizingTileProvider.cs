using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Extensions;
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
    private readonly object renderLock = new object();

    public RasterizingTileProvider(ILayer layer,
        double renderResolutionMultiplier = 1,
        IRenderer? rasterizer = null,
        float pixelDensity = 1,
        IPersistentCache<byte[]>? persistentCache = null)
    {
        _layer = layer;
        _renderResolutionMultiplier = renderResolutionMultiplier;
        _rasterizer = rasterizer;
        _pixelDensity = pixelDensity;
        PersistentCache = persistentCache ?? new NullCache();
    }

    public IPersistentCache<byte[]> PersistentCache { get; set; }

    public byte[]? GetTile(TileInfo tileInfo)
    {
        var result = PersistentCache.Find(tileInfo.Index);
        if (result == null)
        {
            var renderer = GetRenderer();
            Schema.Resolutions.TryGetValue(tileInfo.Index.Level, out var tileResolution);

            var resolution = tileResolution.UnitsPerPixel;
            var viewPort = RasterizingLayer.CreateViewport(tileInfo.Extent.ToMRect(), resolution,
                _renderResolutionMultiplier, 1);
            ILayer renderLayer;
            lock (renderLock)
            {
                var fetchInfo = new FetchInfo(viewPort.Extent, resolution);
                var features = GetFeatures(fetchInfo);
                renderLayer = new MemoryLayer(_layer.Name) 
                {
                    Style = _layer.Style,
                    DataSource = new MemoryProvider<IFeature>(features),
                };
            }

            using var stream = renderer.RenderToBitmapStream(viewPort, new[] { renderLayer }, pixelDensity: _pixelDensity);
            _rasterizingLayers.Push(renderer);
            result = stream?.ToArray();
            PersistentCache?.Add(tileInfo.Index, result ?? Array.Empty<byte>());
        }

        return result;
    }

    private IEnumerable<IFeature> GetFeatures(FetchInfo fetchInfo)
    {
        if (_layer is IDataSourceLayer { DataSource: { } } dataSourceLayer)
        {
            return dataSourceLayer.DataSource.GetFeatures(fetchInfo);
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
}