using System.Collections.Generic;
using BruTile;
using BruTile.Predefined;
using Mapsui.Extensions;
using Mapsui.Rendering;

namespace Mapsui.Layers;

public class RasterizingTileProvider : ITileSource
{
    private readonly Stack<RasterizingLayer> _rasterizingLayers = new();
    private readonly int _delayBeforeRasterize;
    private readonly double _renderResolutionMultiplier;
    private readonly IRenderer? _rasterizer;
    private readonly double _overscanRatio;
    private readonly bool _onlyRerasterizeIfOutsideOverscan;
    private readonly float _pixelDensity;
    private readonly ILayer _layer;
    private ITileSchema? _tileSchema;
    private Attribution? _attribution;

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
        var rasterizerLayer = GetRasterizerLayer();
        var result = rasterizerLayer.Rasterize(new FetchInfo(tileInfo.Extent.ToMRect(), _pixelDensity));
        _rasterizingLayers.Push(rasterizerLayer);
        return result?.Data;
    }

    private RasterizingLayer GetRasterizerLayer()
    {
        var rasterizer = _rasterizingLayers.Pop();
        if (rasterizer == null)
        {
            rasterizer = new RasterizingLayer(_layer, _delayBeforeRasterize, _renderResolutionMultiplier, _rasterizer, _overscanRatio,
                _onlyRerasterizeIfOutsideOverscan, _pixelDensity);
        }

        return rasterizer;
    }

    public ITileSchema Schema => _tileSchema ??= new GlobalSphericalMercator();
    public string Name => _layer.Name;
    public Attribution Attribution => _attribution ??= new Attribution(_layer.Attribution.Text ,_layer.Attribution.Url);
}