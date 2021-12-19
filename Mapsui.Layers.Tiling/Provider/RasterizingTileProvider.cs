using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Extensions;
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
    private FetchInfo? _fetchInfo;

    public RasterizingTileProvider(
        ILayer layer,
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
            var viewPort = RasterizingLayer.CreateViewport(tileInfo.Extent.ToMRect(), _fetchInfo?.Resolution ?? 1,
                _renderResolutionMultiplier, 1);
            using var stream = renderer.RenderToBitmapStream(viewPort, new[] { _layer }, pixelDensity: _pixelDensity);
            _rasterizingLayers.Push(renderer);
            result = stream?.ToArray();
            PersistentCache?.Add(tileInfo.Index, result);
            return result;
        }
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

    public void RefreshData(FetchInfo fetchInfo)
    {
        _fetchInfo = fetchInfo;
    }
}