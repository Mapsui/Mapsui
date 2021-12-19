using System.Collections.Concurrent;
using System.Collections.Generic;
using BruTile;
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
        float pixelDensity = 1)
    {
        _layer = layer;
        _renderResolutionMultiplier = renderResolutionMultiplier;
        _rasterizer = rasterizer;
        _pixelDensity = pixelDensity;
    }

    public byte[]? GetTile(TileInfo tileInfo)
    {
        var renderer = GetRenderer();
        var viewPort = RasterizingLayer.CreateViewport(tileInfo.Extent.ToMRect(), _fetchInfo?.Resolution ?? 1, _renderResolutionMultiplier, 1);
        using var result = renderer.RenderToBitmapStream(viewPort, new[] { _layer }, pixelDensity: _pixelDensity);
        _rasterizingLayers.Push(renderer);
        return result?.ToArray();
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