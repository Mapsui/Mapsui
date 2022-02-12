using System;
using System.Collections.Concurrent;
using System.IO;
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
    private readonly ETileFormat _tileFormat;

    public RasterizingTileProvider(ILayer layer,
        double renderResolutionMultiplier = 1,
        IRenderer? rasterizer = null,
        float pixelDensity = 1,
        IPersistentCache<byte[]>? persistentCache = null,
        ETileFormat tileFormat = ETileFormat.Png)
    {
        _layer = layer;
        _renderResolutionMultiplier = renderResolutionMultiplier;
        _rasterizer = rasterizer;
        _pixelDensity = pixelDensity;
        _tileFormat = tileFormat;
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

            MemoryStream? stream = null;
            try
            {
                switch (_tileFormat)
                {
                    case ETileFormat.Png:
                        stream = renderer.RenderToBitmapStream(viewPort, new[] { _layer }, pixelDensity: _pixelDensity);
                        break;
                    case ETileFormat.Skp:
                        if (renderer is IPictureRenderer pictureRenderer)
                        {
                            stream = pictureRenderer.RenderToPictureStream(viewPort, new[] { _layer });
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

    private IRenderer GetRenderer()
    {
        if (!_rasterizingLayers.TryPop(out var rasterizer))
        {
            rasterizer = (_rasterizer as IPictureRenderer) ?? (IPictureRenderer)DefaultRendererFactory.Create();
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

        Schema.Resolutions.TryGetValue(tileInfo.Index.Level, out var tileResolution);

        var resolution = tileResolution.UnitsPerPixel;
        var viewPort = RasterizingLayer.CreateViewport(tileInfo.Extent.ToMRect(), resolution, _renderResolutionMultiplier, 1);
        var result = renderer.RenderToPicture(viewPort, new[] { _layer });
        _rasterizingLayers.Push(renderer);
        return result;
    }
}