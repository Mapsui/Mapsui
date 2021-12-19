using System.Collections.Generic;
using BruTile;
using BruTile.Predefined;
using Mapsui.Extensions;
using Mapsui.Rendering;

namespace Mapsui.Layers;

public class RasterizingTileProvider : ITileSource
{
    private readonly Stack<IRenderer> _rasterizingLayers = new();
    private readonly int _delayBeforeRasterize;
    private readonly double _renderResolutionMultiplier;
    private readonly IRenderer? _rasterizer;
    private readonly double _overscanRatio;
    private readonly bool _onlyRerasterizeIfOutsideOverscan;
    private readonly float _pixelDensity;
    private readonly ILayer _layer;
    private ITileSchema? _tileSchema;
    private Attribution? _attribution;
    private FetchInfo? _fetchInfo;

    public RasterizingTileProvider(
        ILayer layer,
        int delayBeforeRasterize = 1000,
        double renderResolutionMultiplier = 1,
        IRenderer? rasterizer = null,
        double overscanRatio = 1,
        bool onlyRerasterizeIfOutsideOverscan = false,
        float pixelDensity = 1)
    {
        _layer = layer;
        _delayBeforeRasterize = delayBeforeRasterize;
        _renderResolutionMultiplier = renderResolutionMultiplier;
        _rasterizer = rasterizer;
        _overscanRatio = overscanRatio;
        _onlyRerasterizeIfOutsideOverscan = onlyRerasterizeIfOutsideOverscan;
        _pixelDensity = pixelDensity;
    }

    public byte[]? GetTile(TileInfo tileInfo)
    {
        var renderer = GetRasterizerLayer();
        var viewPort = RasterizingLayer.CreateViewport(tileInfo.Extent.ToMRect(), _fetchInfo?.Resolution ?? 1,  _renderResolutionMultiplier, _overscanRatio);
        using var result = renderer.RenderToBitmapStream(viewPort,new []{ _layer }, pixelDensity: _pixelDensity);
        _rasterizingLayers.Push(renderer);
        return result?.ToArray();
    }

    private IRenderer GetRasterizerLayer()
    {
        var rasterizer = _rasterizingLayers.Pop();
        if (rasterizer == null)
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